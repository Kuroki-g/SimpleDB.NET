using SimpleDB.Tx;

namespace SimpleDB.Structure;

public class TableScan : ITableScan
{
    private readonly ITransaction _tx;

    private readonly ILayout _layout;

    private readonly IRecordPage _recordPage;

    private readonly string _fileName;
    private int _currentSlot;

    public TableScan(ITransaction tx, string tableName, ILayout layout)
    {
        _tx = tx;
        _layout = layout;
        _fileName = $"{tableName}.tbl";
        if (_tx.Size(_fileName) == 0)
        {
            MoveToNewBlock();
        }
        else
        {
            MoveToBlock();
        }
    }

    public void BeforeFirst()
    {
        MoveToBlock(0);
    }

    private void MoveToBlock(int v)
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public int GetInt(string fieldName)
    {
        throw new NotImplementedException();
    }

    public RecordId GetRecordId()
    {
        throw new NotImplementedException();
    }

    public string GetString(string fieldName)
    {
        throw new NotImplementedException();
    }

    public bool HasField(string fieldName)
    {
        throw new NotImplementedException();
    }

    public void Insert()
    {
        throw new NotImplementedException();
    }

    public void MoveToRecordId(RecordId recordId)
    {
        throw new NotImplementedException();
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

    private bool AtLastBlock()
    {
        throw new NotImplementedException();
    }

    public void SetInt(string fieldName, int value)
    {
        throw new NotImplementedException();
    }

    public void SetString(string fieldName, string value)
    {
        throw new NotImplementedException();
    }

    private void MoveToBlock()
    {
        throw new NotImplementedException();
    }

    private void MoveToNewBlock()
    {
        throw new NotImplementedException();
    }
}
