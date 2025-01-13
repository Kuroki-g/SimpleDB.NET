using SimpleDB.Metadata;
using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public class TablePlan : IPlan
{
    private readonly string _tableName;

    private readonly ITransaction _tx;

    private readonly Layout _layout;

    private readonly StatInfo _statInfo;

    public TablePlan(ITransaction tx, string tableName, IMetadataManager manager)
    {
        _tableName = tableName;
        _tx = tx;
        _layout = manager.GetLayout(tableName, tx);
        _statInfo = manager.GetStatInfo(tableName, _layout, tx);
    }

    public IScan Open()
    {
        return new TableScan(_tx, _tableName, _layout);
    }

    public Schema Schema => _layout.Schema;

    public int BlocksAccessed => _statInfo.BlocksAccessed;

    public int RecordsOutput => _statInfo.RecordsOutput;

    public int DistinctValues(string fieldName)
    {
        return _statInfo.DistinctValues(fieldName);
    }
}
