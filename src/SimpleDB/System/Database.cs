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

    public Database(ISimpleDbConfig dbConfig, IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        BlockSize = dbConfig.BlockSize;

        _fm = fm;
        _lm = lm;
        _bm = bm;

        var tx = NewTx();
        var isNew = _fm.IsNew;
        if (isNew)
        {
            Console.WriteLine("creating new database");
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
        return new Transaction(_fm, _lm, _bm);
    }
}
