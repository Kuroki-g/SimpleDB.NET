using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Metadata;

public class StatManagerTest : IntegrationTestBase
{
    [Fact]
    public void Constructor_empty_database()
    {
        var (tm, tx) = CreateTableManager();

        var statManager = StatManager.GetInstance(tm, tx);

        Assert.NotNull(statManager);
    }

    [Fact]
    public void Constructor_not_empty_database()
    {
        var (tm, tx) = CreateTableManager();
        var schema = CreateSchema();
        tm.CreateTable("SampleTable", schema, tx);

        var statManager = StatManager.GetInstance(tm, tx);

        Assert.NotNull(statManager);
    }

    [Fact]
    public void GetStatInfo_insert_increase_block_number()
    {
        var (tm, tx) = CreateTableManager();
        var schema = CreateSchema();
        var layout = new Layout(schema);
        tm.CreateTable("SampleTable", schema, tx);
        var ts = new TableScan(tx, "SampleTable", layout);
        ts.Insert();
        ts.SetInt("A", 1);
        ts.Close();

        var sm = StatManager.GetInstance(tm, tx);
        var actual = sm.GetStatInfo("SampleTable", layout, tx);

        // Block数は別途入れている数に依存し、算出に手間がかかるので、とりあえず1増える分だけ入れている。
        Assert.Equal(1, actual.BlocksAccessed);
        Assert.Equal(1, actual.RecordsOutput);
    }
}
