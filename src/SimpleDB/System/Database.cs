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

    internal readonly IFileManager Fm;

    internal readonly ILogManager Lm;

    internal readonly IBufferManager Bm;

    internal readonly IMetadataManager Mm;

    public readonly Planner Planner;

    internal bool IsDisposed = false;

    /// <summary>
    /// 常に新しいDBを作成する場合に用いるコンストラクタ。
    /// </summary>
    /// <param name="dbConfig"></param>
    internal Database(ISimpleDbConfig dbConfig)
    {
        Fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = dbConfig.FileName,
                FileName = dbConfig.FileName,
                BlockSize = dbConfig.BlockSize,
            }
        );
        Lm = new LogManager(Fm, dbConfig.LogFileName);
        Bm = new BufferManager(Fm, Lm, dbConfig.BufferSize);

        BlockSize = dbConfig.BlockSize;
        Mm = new MetadataManager(true, NewTx());
        Planner = CreatePlanner(Mm);
    }

    public Database(ISimpleDbConfig dbConfig, IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        Fm = fm;
        Lm = lm;
        Bm = bm;

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
        return new Transaction(Fm, Lm, Bm);
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
                if (Bm is IDisposable disposable_bm)
                {
                    disposable_bm.Dispose();
                }
                if (Lm is IDisposable disposable_lm)
                {
                    disposable_lm.Dispose();
                }
                if (Fm is IDisposable disposable_fm)
                {
                    disposable_fm.Dispose();
                }
            }

            IsDisposed = true;
        }
    }

    ~Database()
    {
        Dispose();
    }
}
