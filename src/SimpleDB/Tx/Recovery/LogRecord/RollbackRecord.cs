using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class RollbackRecord : ILogRecord
{
    public TransactionStatus Op => TransactionStatus.ROLLBACK;

    public int TxNumber { get; }

    public RollbackRecord(Page page)
    {
        var tPos = Bytes.Integer;
        TxNumber = page.GetInt(tPos);
    }

    /// <summary>
    /// コミットレコードはundoについての情報を含まないため、何もしない。
    /// </summary>
    /// <param name="tx"></param>
    public void Undo(ITransaction tx) { }

    public override string ToString() => $"<ROLLBACK {TxNumber}>";

    public static int WriteToLog(ILogManager lm, int txNum)
    {
        byte[] record = new byte[2 * Bytes.Integer];

        var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.ROLLBACK);
        p.SetInt(Bytes.Integer, txNum);

        return lm.Append(record);
    }
}
