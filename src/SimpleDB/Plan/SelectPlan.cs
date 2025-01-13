using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Plan;

public class SelectPlan(IPlan plan, Predicate predicate) : IPlan
{
    private readonly IPlan _plan = plan;

    private readonly Predicate _predicate = predicate;

    public Schema Schema => _plan.Schema;

    public int BlocksAccessed => _plan.BlocksAccessed;

    public int RecordsOutput => _plan.RecordsOutput / _predicate.ReductionFactor(_plan);

    public int DistinctValues(string fieldName)
    {
        if (_predicate.EquatesWithField(fieldName) is not null)
        {
            return 1;
        }

        var fieldName2 = _predicate.EquatesWithField(fieldName);
        if (fieldName2 is not null)
        {
            return Math.Min(_plan.DistinctValues(fieldName), _plan.DistinctValues(fieldName2));
        }
        return _plan.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var scan = _plan.Open();
        return new SelectScan((IUpdateScan)scan, _predicate);
    }
}
