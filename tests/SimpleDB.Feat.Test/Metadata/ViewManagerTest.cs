using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;

namespace SimpleDB.Feat.Test.Metadata;

public class ViewManagerTest : IntegrationTestBase
{
    [Fact]
    public void Constructor_new_create_view_catalog_table()
    {
        List<string> expected = [ViewSchema.FIELD_VIEW_DEF, ViewSchema.FIELD_VIEW_NAME];
        var beforeInitialize = _directoryInfo
            .GetFiles()
            .Select((info) => info.Name)
            .Order()
            .ToList();

        var (tm, tx) = CreateTableManager();
        var vm = new ViewManager(true, tm, tx);
        var newLayout = tm.GetLayout(ViewSchema.TABLE_NAME_VIEW_CATALOG, tx);

        var actual = newLayout.Schema.Fields.Order().ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateView_GetViewDef()
    {
        using var tx = CreateTransaction();
        var tm = new TableManager(true, tx);
        var vm = new ViewManager(true, tm, tx);
        tx.Commit();

        var expected = "CREATE VIEW AS SELECT * FROM `test`;";
        vm.CreateView("sample-view", expected, tx);
        tx.Commit();

        var actual = vm.GetViewDef("sample-view", tx);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetViewDef_empty()
    {
        using var tx = CreateTransaction();
        var tm = new TableManager(true, tx);
        var vm = new ViewManager(true, tm, tx);
        tx.Commit();

        var actual = vm.GetViewDef("sample-view", tx);

        Assert.Equal(string.Empty, actual);
    }
}
