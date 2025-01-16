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
    private readonly IFileManager _fm;
    private readonly ILogManager _lm;
    private readonly IBufferManager _bm;
    private readonly IMetadataManager _mm;

    public readonly Planner Planner;

    public Database(ISimpleDbConfig dbConfig)
    {
        BlockSize = dbConfig.BlockSize;

        _fm = new FileManager(dbConfig.FileName, BlockSize);
        _lm = new LogManager(_fm, dbConfig.LogFileName);
        _bm = new BufferManager(_fm, _lm, dbConfig.BufferSize);

        var tx = NewTx();
        var isNew = _fm.IsNew;
        if (isNew)
        {
            Console.WriteLine("creating new database");
            tx.Recover();
        }
        else
        {
            Console.WriteLine("recovering existing database");
            tx.Recover();
        }
        // TODO: planner switch
        _mm = new MetadataManager(isNew, tx);
        Planner = CreatePlanner(_mm);
        tx.Commit();
    }

    private static Planner CreatePlanner(IMetadataManager mm)
    {
        var updatePlanner = new BasicUpdatePlanner(mm);
        var queryPlanner = new BasicQueryPlanner(mm);
        return new Planner(queryPlanner, updatePlanner);
    }

    public ITransaction NewTx()
    {
        return new Transaction(_fm, _lm, _bm);
    }
}
