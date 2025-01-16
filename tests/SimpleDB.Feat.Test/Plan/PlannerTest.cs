using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Plan;

namespace SimpleDB.Feat.Test.Plan;

public class PlannerTest : IntegrationTestBase
{
    [Fact]
    public void CreatePlan()
    {
        var tx = CreateTransaction();
        var mm = new MetadataManager(true, tx);
        var updatePlanner = new BasicUpdatePlanner(mm);
        var queryPlanner = new BasicQueryPlanner(mm);
        var planner = new Planner(queryPlanner, updatePlanner);
    }

    [Fact]
    public void ExecuteUpdateCmd()
    {
        var tx = CreateTransaction();
        var mm = new MetadataManager(true, tx);
        var updatePlanner = new BasicUpdatePlanner(mm);
        var queryPlanner = new BasicQueryPlanner(mm);
        var planner = new Planner(queryPlanner, updatePlanner);

        planner.ExecuteUpdateCmd("create table test (a int)", tx);
    }
}
