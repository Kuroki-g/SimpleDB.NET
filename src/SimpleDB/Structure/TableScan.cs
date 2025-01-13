using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.Structure;

/// <summary>
/// TODO: IEnumerableを実装したほうがよい。
/// </summary>
public class TableScan : ITableScan
{
    private readonly ITransaction _tx;

    private readonly Layout _layout;

    private RecordPage _recordPage;

    private readonly string _fileName;
    private int _currentSlot;

    public static string RealFileName(string tableName) => $"{tableName}.tbl";

    public TableScan(ITransaction tx, string tableName, Layout layout)
    {
        _tx = tx;
        _layout = layout;
        _fileName = RealFileName(tableName);
        if (_tx.Size(_fileName) == 0)
        {
            MoveToNewBlock();
        }
        else
        {
            MoveToBlock(0); // TODO: beforeFirstを呼ぶべきではないか？
        }
    }

    public void BeforeFirst() => MoveToBlock(0);

    private void MoveToBlock(int blockNumber)
    {
        Close();
        var blockId = new BlockId(_fileName, blockNumber);
        _recordPage = new RecordPage(_tx, blockId, _layout);
        _currentSlot = -1;
    }

    public void Close()
    {
        if (_recordPage is not null)
        {
            _tx.Unpin(_recordPage.BlockId);
        }
    }

    public int GetInt(string fieldName) => _recordPage.GetInt(_currentSlot, fieldName);

    public RecordId GetRecordId() => new(_recordPage.BlockId.Number, _currentSlot);

    public string GetString(string fieldName) => _recordPage.GetString(_currentSlot, fieldName);

    public bool HasField(string fieldName) => _layout.Schema.HasField(fieldName);

    public void Insert()
    {
        _currentSlot = _recordPage.InsertAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
            {
                MoveToNewBlock();
            }
            else
            {
                MoveToBlock(_recordPage.BlockId.Number + 1);
            }
            _currentSlot = _recordPage.InsertAfter(_currentSlot);
        }
    }

    public void Delete() => _recordPage.Delete(_currentSlot);

    public void MoveToRecordId(RecordId recordId)
    {
        Close();
        var blockId = new BlockId(_fileName, recordId.BlockNumber);
        _recordPage = new RecordPage(_tx, blockId, _layout);
        _currentSlot = recordId.Slot;
    }

    public bool Next()
    {
        _currentSlot = _recordPage.NextAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
                return false;
            MoveToBlock(_recordPage.BlockId.Number + 1);
            _currentSlot = _recordPage.NextAfter(_currentSlot);
        }
        return true;
    }

    private bool AtLastBlock() => _recordPage.BlockId.Number == _tx.Size(_fileName) - 1;

    public void SetInt(string fieldName, int value) =>
        _recordPage.SetInt(_currentSlot, fieldName, value);

    public void SetString(string fieldName, string value) =>
        _recordPage.SetString(_currentSlot, fieldName, value);

    private void MoveToNewBlock()
    {
        Close();
        var blockId = _tx.Append(_fileName);
        _recordPage = new RecordPage(_tx, blockId, _layout);
        _recordPage.Format();

        _currentSlot = -1;
    }

    public Constant GetValue(string fieldName)
    {
        return _layout.Schema.Type(fieldName) == Types.INTEGER
            ? new Constant(GetInt(fieldName))
            : new Constant(GetString(fieldName));
    }
}
