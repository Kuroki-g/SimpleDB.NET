using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Plan;

public class ProductPlan : IPlan
{
    private readonly IPlan _plan1;

    private readonly IPlan _plan2;

    private readonly Schema _schema = new();

    public ProductPlan(IPlan plan1, IPlan plan2)
    {
        _plan1 = plan1;
        _plan2 = plan2;
        _schema.AddAll(plan1.Schema);
        _schema.AddAll(plan2.Schema);
    }

    public Schema Schema => _schema;

    public int BlocksAccessed
    {
        get { return _plan1.BlocksAccessed + _plan2.BlocksAccessed; }
    }

    public int RecordsOutput
    {
        get { return _plan1.RecordsOutput * _plan2.RecordsOutput; }
    }

    public int DistinctValues(string fieldName)
    {
        return _plan1.Schema.HasField(fieldName)
            ? _plan1.DistinctValues(fieldName)
            : _plan2.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var s1 = _plan1.Open();
        var s2 = _plan2.Open();
        return new ProductScan(s1, s2);
    }
}
