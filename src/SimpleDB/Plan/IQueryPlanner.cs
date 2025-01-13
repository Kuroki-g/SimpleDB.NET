using SimpleDB.SqlParser.Grammar;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public interface IQueryPlanner
{
    public IPlan CreatePlan(Query query, ITransaction tx);
}
