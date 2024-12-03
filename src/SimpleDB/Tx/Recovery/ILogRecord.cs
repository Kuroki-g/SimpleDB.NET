using SimpleDB.Logging;

namespace SimpleDB.Tx.Recovery;

public interface ILogRecord
{
    internal TransactionStatus Op { get; }

    internal int TxNumber { get; }

    public string ToString();

    public void Undo(Transaction tx);
}
