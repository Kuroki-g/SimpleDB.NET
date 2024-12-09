using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class SetStringRecord : ILogRecord
{
    private readonly int _txNumber;

    private readonly int _offset;

    private readonly BlockId _blockId;

    private readonly string _value;

    public TransactionStatus Op => TransactionStatus.SETSTRING;

    public int TxNumber => _txNumber;

    public SetStringRecord(Page page)
    {
        int tPos = Bytes.Integer;
        _txNumber = page.GetInt(tPos);

        int fPos = tPos + Bytes.Integer;
        string fileName = page.GetString(fPos);

        int bPos = fPos + Page.MaxLength(fileName.Length);
        int blockNumber = page.GetInt(0);
        _blockId = new BlockId(fileName, blockNumber);

        int oPos = bPos + Bytes.Integer;
        _offset = page.GetInt(oPos);

        int vPos = oPos + Bytes.Integer;
        _value = page.GetString(vPos);
    }

    public override string ToString() => $"<SETSTRING {_txNumber} {_blockId} {_offset} {_value}>";

    public void Undo(ITransaction tx)
    {
        tx.Pin(_blockId);
        tx.SetString(_blockId, _offset, _value, false); // don't log the undo!
        tx.Unpin(_blockId);
    }

    public static int WriteToLog(ILogManager lm, int txnum, BlockId blk, int offset, string val)
    {
        int tPos = Bytes.Integer;
        int fPos = tPos + Bytes.Integer;
        int bPos = fPos + Page.MaxLength(blk.FileName.Length);
        int oPos = bPos + Bytes.Integer;
        int vPos = oPos + Bytes.Integer;
        int recordLength = vPos + Page.MaxLength(val.Length);
        byte[] record = new byte[recordLength];

        var p = new Page(record);
        p.SetInt(0, (int)TransactionStatus.SETSTRING);
        p.SetInt(tPos, txnum);
        p.SetString(fPos, blk.FileName);
        p.SetInt(bPos, blk.Number);
        p.SetInt(oPos, offset);
        p.SetString(vPos, val);

        return lm.Append(record);
    }
}
