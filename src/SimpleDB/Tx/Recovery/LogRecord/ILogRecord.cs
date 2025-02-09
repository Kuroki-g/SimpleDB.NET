using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public interface ILogRecord
{
    /// <summary>
    /// ログレコードの種類を返す。
    /// </summary>
    public TransactionStatus Op { get; }

    /// <summary>
    /// トランザクション番号を返す。
    /// </summary>
    internal int TxNumber { get; }

    public string ToString();

    /// <summary>
    /// ログレコードによって指定のundoを行う。
    /// </summary>
    /// <param name="tx"></param>
    public void Undo(ITransaction tx);

    public static ILogRecord Create(byte[] record)
    {
        if (record.Length == 0)
        {
            return new CheckpointRecord(); // is it ok?
        }
        var p = new Page(record);
        var status = (TransactionStatus)p.GetInt(0);
        return status switch
        {
            TransactionStatus.CHECKPOINT => new CheckpointRecord(),
            TransactionStatus.START => new StartRecord(p),
            TransactionStatus.COMMIT => new CommitRecord(p),
            TransactionStatus.ROLLBACK => new RollbackRecord(p),
            TransactionStatus.SETINT => new SetIntRecord(p),
            TransactionStatus.SETSTRING => new SetStringRecord(p),
            _ => throw new SystemException("invalid log record type"),
        };
    }
}
