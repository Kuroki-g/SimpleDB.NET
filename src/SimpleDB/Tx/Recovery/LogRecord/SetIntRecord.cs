using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Recovery.LogRecord;

public sealed class SetIntRecord : ILogRecord
{
    public TransactionStatus Op => TransactionStatus.SETINT;

    private readonly int _offset;

    private readonly int _value;

    private readonly BlockId _blockId;

    public SetIntRecord(Page page)
    {
        var tPos = Bytes.Integer;
        TxNumber = page.GetInt(tPos);

        var fPos = tPos + Bytes.Integer;
        var fileName = page.GetString(fPos);

        var bPos = fPos + Page.MaxLength(fileName.Length);
        var blockNumber = page.GetInt(bPos);
        _blockId = new BlockId(fileName, blockNumber);

        var oPos = bPos + Bytes.Integer;
        _offset = page.GetInt(oPos);

        var vPos = oPos + Bytes.Integer;
        _value = page.GetInt(vPos);
    }

    public override string ToString() => $"<SETINT {TxNumber} {_blockId} {_offset} {_value}>";

    public int TxNumber { get; }

    public void Undo(ITransaction tx)
    {
        tx.Pin(_blockId);
        tx.SetInt(_blockId, _offset, _value, false);
        tx.Pin(_blockId);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lm"></param>
    /// <param name="txNum"></param>
    /// <param name="blockId"></param>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns>ログ シーケンス番号 (LSN)</returns>
    public static int WriteToLog(ILogManager lm, int txNum, BlockId blockId, int offset, int value)
    {
        var tPos = Bytes.Integer;
        var fPos = tPos + Bytes.Integer;
        var bPos = fPos + Page.MaxLength(blockId.FileName.Length);
        var oPos = bPos + Bytes.Integer;
        var vPos = oPos + Bytes.Integer;

        var record = new byte[vPos + Bytes.Integer];
        using var page = new Page(record);
        page.SetInt(0, (int)TransactionStatus.SETINT);
        page.SetInt(tPos, txNum);
        page.SetString(fPos, blockId.FileName);
        page.SetInt(bPos, blockId.Number);
        page.SetInt(oPos, offset);
        page.SetInt(vPos, value);

        return lm.Append(record);
    }
}
