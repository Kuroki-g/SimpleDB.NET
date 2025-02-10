using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Metadata;
using SimpleDB.Plan;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.System;

public sealed class Database
{
    public readonly int BlockSize;

    internal readonly IFileManager Fm;

    private readonly ILogManager _lm;

    private readonly IBufferManager _bm;

    internal readonly IMetadataManager Mm;

    public readonly Planner Planner;

    /// <summary>
    /// 常に新しいDBを作成する場合に用いるコンストラクタ。
    /// </summary>
    /// <param name="dbConfig"></param>
    internal Database(ISimpleDbConfig dbConfig)
    {
        Fm = new FileManager(dbConfig.FileName, dbConfig.BlockSize);
        _lm = new LogManager(Fm, dbConfig.LogFileName);
        _bm = new BufferManager(Fm, _lm, dbConfig.BufferSize);

        BlockSize = dbConfig.BlockSize;
        Mm = new MetadataManager(true, NewTx());
        Planner = CreatePlanner(Mm);
    }

    public Database(ISimpleDbConfig dbConfig, IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        Fm = fm;
        _lm = lm;
        _bm = bm;

        var tx = NewTx();
        var isNew = Fm.IsNew;
        if (isNew)
        {
            Console.WriteLine("creating new database");
        }
        else
        {
            Console.WriteLine("recovering existing database");
            tx.Recover();
        }

        BlockSize = dbConfig.BlockSize;
        // TODO: planner switch
        Mm = new MetadataManager(isNew, tx);
        Planner = CreatePlanner(Mm);
        tx.Commit();
        tx.Dispose();
        Console.WriteLine("recovery complete");
    }

    private static Planner CreatePlanner(IMetadataManager mm)
    {
        var updatePlanner = new BasicUpdatePlanner(mm);
        var queryPlanner = new BasicQueryPlanner(mm);
        return new Planner(queryPlanner, updatePlanner);
    }

    public ITransaction NewTx()
    {
        return new Transaction(Fm, _lm, _bm);
    }
}
