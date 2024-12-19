using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Structure;
using SimpleDB.Tx;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Tx;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly string _dir;

    protected readonly DirectoryInfo _directoryInfo;

    public IntegrationTestBase()
    {
        var randomStr = Helper.RandomString(12);
        _dir = $"./mock/{randomStr}";
        _directoryInfo = new(_dir);

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
        var fm = new FileManager(_dir, 0x190);
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
}
