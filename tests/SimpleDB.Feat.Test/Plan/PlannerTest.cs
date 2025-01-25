using FakeItEasy;
using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Plan;
using SimpleDB.SqlParser.Grammar.Create;
using SimpleDB.SqlParser.Grammar.UpdateCmd;

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

        Assert.IsType<ProjectPlan>(plan);
    }

    [Fact]
    public void ExecuteUpdateCmd_Insert()
    {
        var tx = CreateTransaction();
        var up = A.Fake<IUpdatePlanner>();
        var qp = A.Fake<IQueryPlanner>();

        var planner = new Planner(qp, up);

        var cmd = "insert into T1(A,B) values(1, 'test')";
        planner.ExecuteUpdateCmd(cmd, tx);

        A.CallTo(() => up.ExecuteInsert(A<Insert>._, tx)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ExecuteUpdateCmd_Delete()
    {
        var tx = CreateTransaction();
        var up = A.Fake<IUpdatePlanner>();
        var qp = A.Fake<IQueryPlanner>();

        var planner = new Planner(qp, up);

        var cmd = "delete from T1 where A=1";
        planner.ExecuteUpdateCmd(cmd, tx);

        A.CallTo(() => up.ExecuteDelete(A<Delete>._, tx)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ExecuteUpdateCmd_CreateTable()
    {
        var tx = CreateTransaction();
        var up = A.Fake<IUpdatePlanner>();
        var qp = A.Fake<IQueryPlanner>();

        var planner = new Planner(qp, up);

        var cmd = "create table T2(A int, B varchar(9))";
        planner.ExecuteUpdateCmd(cmd, tx);

        A.CallTo(() => up.ExecuteCreateTable(A<CreateTable>._, tx)).MustHaveHappenedOnceExactly();
    }
}
