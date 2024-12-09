using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class CheckpointRecord : ILogRecord
{
    public CheckpointRecord() { }

    public TransactionStatus Op => TransactionStatus.CHECKPOINT;

    public int TxNumber => -1;

    public void Undo(ITransaction tx) { }

    public override string ToString() => "<CHECKPOINT>";

    public static int WriteToLog(ILogManager lm)
    {
        var record = new byte[Bytes.Integer];

        var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.CHECKPOINT);

        return lm.Append(record);
    }
}
