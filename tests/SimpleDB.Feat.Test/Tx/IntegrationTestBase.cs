using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Metadata;
using SimpleDB.Sql;
using SimpleDB.Storage;
using SimpleDB.Structure;
using SimpleDB.Tx;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Tx;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly Random _random;

    /// <summary>
    /// Real test project directory.
    /// </summary>
    protected readonly string _testProjectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../")
    );

    protected readonly string _dir;

    protected readonly DirectoryInfo _directoryInfo;

    public IntegrationTestBase()
    {
        var randomStr = Helper.RandomString(12);
        _dir = $"./mock/{randomStr}";
        _directoryInfo = new(_dir);

        _random = new Random();

        Helper.InitializeDir(_directoryInfo);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Helper.Delete(_dir);
    }

    /// <summary>
    /// 適度なサイズのバッファやブロックサイズを割り振ったトランザクションを作成する。
    /// </summary>
    /// <returns></returns>
    public Transaction CreateTransaction()
    {
        var fm = new FileManager(_dir, 0x320);
        var lm = new LogManager(fm, "simpledb.log");
        var bm = new BufferManager(fm, lm, 8);

        return new Transaction(fm, lm, bm);
    }

    internal static Schema CreateSchema()
    {
        var schema = new Schema();
        schema.AddIntField("A");
        schema.AddStringField("B", 9);

        return schema;
    }

    internal (TableManager tm, Transaction tx) CreateTableManager(bool isNew = true)
    {
        var tx = CreateTransaction();
        var tm = new TableManager(isNew, tx);

        return (tm, tx);
    }

    internal void CreateSampleTable(string tableName, Schema schema)
    {
        using var tx = CreateTransaction();
        var tm = new TableManager(true, tx);
        tm.CreateTable(tableName, schema, tx);
        tx.Commit();
    }

    internal static List<string> GetStrings(IScan scan, string fieldName)
    {
        var result = new List<string>();
        while (scan.Next())
        {
            result.Add(scan.GetString(fieldName));
        }

        return result;
    }
}
