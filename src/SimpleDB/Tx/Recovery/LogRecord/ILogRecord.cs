using SimpleDB.Logging;

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
}
