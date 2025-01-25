using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class CommitRecord : ILogRecord
{
    public TransactionStatus Op => TransactionStatus.COMMIT;

    public int TxNumber { get; }

    public CommitRecord(Page page)
    {
        var tPos = Bytes.Integer;
        TxNumber = page.GetInt(tPos);
    }

    /// <summary>
    /// コミットレコードはundoについての情報を含まないため、何もしない。
    /// </summary>
    /// <param name="tx"></param>
    public void Undo(ITransaction tx) { }

    public override string ToString() => $"<COMMIT {TxNumber}>";

    public static int WriteToLog(ILogManager lm, int txNum)
    {
        byte[] record = new byte[2 * Bytes.Integer];

        var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.COMMIT);
        p.SetInt(Bytes.Integer, txNum);

        return lm.Append(record);
    }
}
