using SimpleDB.Storage;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Tx.Recovery;

public class RecordFactory
{
    public static ILogRecord? Create(byte[] bytes)
    {
        var p = new Page(bytes);

        var status = (TransactionStatus)p.GetInt(0);
        return status switch
        {
            TransactionStatus.CHECKPOINT => new CheckpointRecord(),
            TransactionStatus.START => new StartRecord(p),
            TransactionStatus.COMMIT => new CommitRecord(p),
            TransactionStatus.ROLLBACK => new RollbackRecord(p),
            TransactionStatus.SETINT => new SetIntRecord(p),
            TransactionStatus.SETSTRING => new SetStringRecord(p),
            _ => null,
        };
    }
}
