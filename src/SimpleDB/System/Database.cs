using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Metadata;
using SimpleDB.Plan;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.System;

public sealed class Database : IDisposable
{
    public readonly int BlockSize;

    private readonly IFileManager _fm;

    private readonly ILogManager _lm;

    private readonly IBufferManager _bm;

    internal readonly IMetadataManager Mm;

    public readonly Planner Planner;

    internal bool IsDisposed = false;

    /// <summary>
    /// 常に新しいDBを作成する場合に用いるコンストラクタ。
    /// </summary>
    /// <param name="dbConfig"></param>
    internal Database(ISimpleDbConfig dbConfig)
    {
        _fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = dbConfig.FileName,
                FileName = dbConfig.FileName,
                BlockSize = dbConfig.BlockSize,
            }
        );
        _lm = LogManager.GetInstance(_fm, dbConfig.LogFileName);
        _bm = BufferManager.GetInstance(_fm, _lm, dbConfig.BufferSize);

        BlockSize = dbConfig.BlockSize;
        Mm = new MetadataManager(true, NewTx());
        Planner = CreatePlanner(Mm);
    }

    public Database(ISimpleDbConfig dbConfig, IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        _fm = fm;
        _lm = lm;
        _bm = bm;

        using var tx = NewTx();
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
        Console.WriteLine("recovery complete");

        BlockSize = dbConfig.BlockSize;
        // TODO: planner switch
        Mm = new MetadataManager(isNew, tx);
        Planner = CreatePlanner(Mm);
        tx.Commit();
        tx.Dispose();
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                _bm.Dispose();
                _lm.Dispose();
                _fm.Dispose();
            }

            IsDisposed = true;
        }
    }

    ~Database()
    {
        Dispose();
    }
}
