using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class StartRecord : ILogRecord
{
    private readonly int _txNum;

    public StartRecord(Page page)
    {
        var tPos = Bytes.Integer;
        _txNum = page.GetInt(tPos);
    }

    public TransactionStatus Op => TransactionStatus.START;

    public int TxNumber => _txNum;

    /// <summary>
    /// Start recordはundoする情報を含まないため何もしない。
    /// </summary>
    /// <param name="tx"></param>
    public void Undo(ITransaction tx) { }

    public override string ToString() => $"<START {_txNum}>";

    public static int WriteToLog(ILogManager lm, int txNum)
    {
        byte[] record = new byte[2 * Bytes.Integer];

        using var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.START);
        p.SetInt(Bytes.Integer, txNum);

        return lm.Append(record);
    }
}
