using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class CommitRecord : ILogRecord
{
    private readonly int _txNum;

    public TransactionStatus Op => TransactionStatus.COMMIT;

    public int TxNumber => _txNum;

    public CommitRecord(Page page)
    {
        var tPos = Bytes.Integer;
        _txNum = page.GetInt(tPos);
    }

    /// <summary>
    /// コミットレコードはundoについての情報を含まないため、何もしない。
    /// </summary>
    /// <param name="tx"></param>
    public void Undo(ITransaction tx) { }

    public override string ToString() => $"<COMMIT {_txNum}>";

    public static int WriteToLog(ILogManager lm, int txNum)
    {
        byte[] record = new byte[2 * Bytes.Integer];

        var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.COMMIT);
        p.SetInt(Bytes.Integer, txNum);

        return lm.Append(record);
    }
}
