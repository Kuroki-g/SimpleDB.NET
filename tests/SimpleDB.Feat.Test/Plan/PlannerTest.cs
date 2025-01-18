using FakeItEasy.Sdk;
using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Plan;

namespace SimpleDB.Feat.Test.Plan;

public class PlannerTest : IntegrationTestBase
{
    [Fact]
    public void CreatePlan()
    {
        {
            CreateSampleTable("T1", CreateSchema());
        }
        var tx = CreateTransaction();
        var mm = new MetadataManager(true, tx);
        var updatePlanner = new BasicUpdatePlanner(mm);
        var queryPlanner = new BasicQueryPlanner(mm);
        var planner = new Planner(queryPlanner, updatePlanner);

        var plan = planner.CreatePlan("select B from T1 where A=10", tx);

        Assert.IsType<SelectPlan>(plan);
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
