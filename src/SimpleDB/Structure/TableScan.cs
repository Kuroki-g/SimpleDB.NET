using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.Structure;

/// <summary>
/// TODO: IEnumerableを実装したほうがよい。
/// </summary>
public class TableScan
    : IDisposable,
        ITableScan,
        // TODO: UpdateScanの場合ではない場合には大丈夫か？
        IUpdateScan
{
    private readonly ITransaction _tx;

    private readonly Layout _layout;

    private RecordPage? _recordPage = null;

    private RecordPage RecordPage =>
        _recordPage ?? throw new InvalidOperationException("RecordPage is not initialized.");

    private readonly string _fileName;
    private int _currentSlot;

    private bool _disposed = false;

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
            // NOTE: beforeFirstは呼び出しと同じことをしているので呼び出し不要
            MoveToBlock(0);
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
        if (_recordPage != null)
        {
            _tx.Unpin(RecordPage.BlockId);
            _recordPage.Dispose(); // Closeしたら初期化する。
            _recordPage = null; // 明示的にnullを代入する。
        }
    }

    public int GetInt(string fieldName) => RecordPage.GetInt(_currentSlot, fieldName);

    public RecordId GetRecordId() => new(RecordPage.BlockId.Number, _currentSlot);

    public string GetString(string fieldName) => RecordPage.GetString(_currentSlot, fieldName);

    public Constant GetValue(string fieldName)
    {
        return _layout.Schema.Type(fieldName) == Types.INTEGER
            ? new Constant(GetInt(fieldName))
            : new Constant(GetString(fieldName));
    }

    public bool HasField(string fieldName) => _layout.Schema.HasField(fieldName);

    public void Insert()
    {
        _currentSlot = RecordPage.InsertAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
            {
                MoveToNewBlock();
            }
            else
            {
                MoveToBlock(RecordPage.BlockId.Number + 1);
            }
            _currentSlot = RecordPage.InsertAfter(_currentSlot);
        }
    }

    public void Delete() => RecordPage.Delete(_currentSlot);

    public void MoveToRecordId(RecordId recordId)
    {
        Close();
        var blockId = new BlockId(_fileName, recordId.BlockNumber);
        _recordPage = new RecordPage(_tx, blockId, _layout);
        _currentSlot = recordId.Slot;
    }

    public bool Next()
    {
        _currentSlot = RecordPage.NextAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
                return false;
            MoveToBlock(RecordPage.BlockId.Number + 1);
            _currentSlot = RecordPage.NextAfter(_currentSlot);
        }
        return true;
    }

    private bool AtLastBlock() => RecordPage.BlockId.Number == _tx.Size(_fileName) - 1;

    public void SetInt(string fieldName, int value) =>
        RecordPage.SetInt(_currentSlot, fieldName, value);

    public void SetString(string fieldName, string value) =>
        RecordPage.SetString(_currentSlot, fieldName, value);

    public void SetValue(string fieldName, Constant val)
    {
        if (_layout.Schema.Type(fieldName) == Types.INTEGER)
            SetInt(fieldName, (int)val.AsInt()!);
        else
            SetString(fieldName, val.AsString()!);
    }

    private void MoveToNewBlock()
    {
        Close();
        var blockId = _tx.Append(_fileName);
        _recordPage = new RecordPage(_tx, blockId, _layout);
        _recordPage.Format();

        _currentSlot = -1;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Closeメソッドを呼び出して、確実に_recordPageをDisposeし、Unpinする。
            Close();
            RecordPage.Dispose();
            _tx?.Dispose();
        }

        _disposed = true;
    }
}
