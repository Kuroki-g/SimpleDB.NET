using SimpleDB.Sql;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.Structure;

public class RecordPage
{
    private readonly ITransaction _tx;
    private readonly Layout _layout;

    public BlockId BlockId { get; }

    public RecordPage(ITransaction tx, BlockId blockId, Layout layout)
    {
        _tx = tx;
        BlockId = blockId;
        _layout = layout;
        tx.Pin(blockId);
    }

    public void Delete(int slot) => SetFlag(slot, RecordStatus.EMPTY);

    public void Format()
    {
        var slot = 0;
        while (IsValidSlot(slot))
        {
            _tx.SetInt(BlockId, Offset(slot), RecordStatus.EMPTY, false);
            var schema = _layout.Schema;

            foreach (var fieldName in schema.Fields)
            {
                var fieldPos = Offset(slot) + _layout.Offset(fieldName);

                if (schema.Type(fieldName) == Types.INTEGER)
                {
                    _tx.SetInt(BlockId, fieldPos, default, false);
                }
                else
                {
                    _tx.SetString(BlockId, fieldPos, string.Empty, false);
                }
            }
            slot++;
        }
    }

    /// <summary>
    /// 指定のフィールドに体操するintegerの値を返す。
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public int GetInt(int slot, string fieldName)
    {
        var fieldPos = Offset(slot) + _layout.Offset(fieldName);

        return _tx.GetInt(BlockId, fieldPos);
    }

    public string GetString(int slot, string fieldName)
    {
        var fieldPos = Offset(slot) + _layout.Offset(fieldName);

        return _tx.GetString(BlockId, fieldPos);
    }

    public int InsertAfter(int slot)
    {
        var newSlot = SearchAfter(slot, RecordStatus.EMPTY);
        if (newSlot >= 0)
        {
            SetFlag(newSlot, RecordStatus.USED);
        }
        return newSlot;
    }

    private void SetFlag(int slot, int flag)
    {
        _tx.SetInt(BlockId, Offset(slot), flag, true);
    }

    /// <summary>
    /// flagの値のslotを検索する。
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="flag"></param>
    /// <returns>指定のフラグの値が存在しない場合には-1を返す</returns>
    private int SearchAfter(int slot, int flag)
    {
        slot++;
        while (IsValidSlot(slot))
        {
            if (_tx.GetInt(BlockId, Offset(slot)) == flag)
            {
                return slot;
            }
            slot++;
        }

        return -1;
    }

    public int NextAfter(int slot) => SearchAfter(slot, RecordStatus.USED);

    public void SetInt(int slot, string fieldName, int value)
    {
        var fieldPos = Offset(slot) + _layout.Offset(fieldName);
        _tx.SetInt(BlockId, fieldPos, value, true);
    }

    public void SetString(int slot, string fieldName, string value)
    {
        var fieldPos = Offset(slot) + _layout.Offset(fieldName);
        _tx.SetString(BlockId, fieldPos, value, true);
    }

    private bool IsValidSlot(int slot) => Offset(slot + 1) <= _tx.BlockSize();

    private int Offset(int slot) => slot * _layout.SlotSize;
}
