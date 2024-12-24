using SimpleDB.Sql;
using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public class IndexInfo
{
    private readonly string _indexName;
    private readonly string _fieldName;
    private readonly ISchema _tableSchema;
    private readonly ITransaction _tx;
    private readonly StatInfo _statInfo;
    private readonly Layout _layout;

    public IndexInfo(
        string indexName,
        string fieldName,
        ISchema tableSchema,
        ITransaction tx,
        StatInfo statInfo
    )
    {
        _indexName = indexName;
        _fieldName = fieldName;
        _tableSchema = tableSchema;
        _tx = tx;
        _statInfo = statInfo;
        _layout = CreateIdxLayout();
    }

    public int BlocksAccessed
    {
        get
        {
            var rpb = _tx.BlockSize() / _layout.SlotSize;
            var blockCount = _statInfo.RecordsOutput / rpb;
            return HashIndex.SearchCost(blockCount, rpb);
        }
    }

    public int RecordsOutput => _statInfo.RecordsOutput / _statInfo.DistinctValues(_fieldName);

    public int DistinctValues(string fieldName) =>
        _fieldName.Equals(fieldName) ? _statInfo.DistinctValues(_fieldName) : 1;

    public IIndex Open()
    {
        return new HashIndex(_tx, _indexName, _layout);
    }

    private Layout CreateIdxLayout()
    {
        var schema = new IndexSchema(_fieldName);

        return new Layout(schema);
    }
}
