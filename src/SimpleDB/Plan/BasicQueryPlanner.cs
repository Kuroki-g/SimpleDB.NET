using SimpleDB.Metadata;
using SimpleDB.SqlParser;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public class BasicQueryPlanner(IMetadataManager mm) : IQueryPlanner
{
    private readonly IMetadataManager _mm = mm;

    public IPlan CreatePlan(Query query, ITransaction tx)
    {
        List<IPlan> plans = query
            .Tables.Select(tableName =>
            {
                var viewDef = _mm.GetViewDef(tableName, tx);
                if (viewDef == string.Empty)
                {
                    return new TablePlan(tx, tableName, _mm);
                }

                var parser = new Parser(viewDef);
                var query = parser.Query();
                return CreatePlan(query, tx);
            })
            .ToList();

        var plan = plans.First();
        foreach (var p in plans.Skip(1))
        {
            plan = new ProductPlan(plan, p);
        }

        var selectPlan = new SelectPlan(plan, query.Predicate);

        return new ProjectPlan(selectPlan, query.Fields);
    }
}
