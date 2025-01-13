using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Plan;

public class ProjectPlan : IPlan
{
    private readonly IPlan _plan;

    public Schema Schema { get; } = new();

    public ProjectPlan(IPlan plan, IEnumerable<string> fieldList)
    {
        _plan = plan;
        foreach (var fieldName in fieldList)
        {
            Schema.Add(fieldName, plan.Schema);
        }
    }

    public int BlocksAccessed => _plan.BlocksAccessed;

    public int RecordsOutput => _plan.RecordsOutput;

    public int DistinctValues(string fieldName)
    {
        return _plan.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var scan = _plan.Open();
        return new ProjectScan(scan, [.. Schema.Fields]);
    }
}
