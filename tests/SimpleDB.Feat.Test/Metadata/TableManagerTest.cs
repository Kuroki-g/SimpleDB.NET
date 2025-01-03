using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Metadata;

public class TableManagerTest : IntegrationTestBase
{
    [Fact]
    public void Constructor_new_database_create_catalog_database()
    {
        List<string> expected =
        [
            TableScan.RealFileName(TableManager.CATALOG_NAME_FIELD),
            TableScan.RealFileName(TableManager.CATALOG_NAME_TABLE),
        ];
        var tx = CreateTransaction();
        var beforeInitialize = _directoryInfo
            .GetFiles()
            .Select((info) => info.Name)
            .Order()
            .ToList();

        var tm = new TableManager(true, tx);

        var actual = _directoryInfo
            .GetFiles()
            .Where((info) => !beforeInitialize.Contains(info.Name))
            .Select((info) => info.Name)
            .Order()
            .ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Constructor_old_database_not_create_catalog_database()
    {
        List<string> expected = [];
        var tx = CreateTransaction();
        var beforeInitialize = _directoryInfo.GetFiles().Select((info) => info.Name).ToList();

        var tm = new TableManager(false, tx);

        var actual = _directoryInfo
            .GetFiles()
            .Where((info) => !beforeInitialize.Contains(info.Name))
            .Select((info) => info.Name)
            .ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateTable_new_table()
    {
        var schema = CreateSchema();
        var tx = CreateTransaction();
        var tm = new TableManager(true, tx);

        tm.CreateTable("MyTable", schema, tx);

        tx.Commit();

        var tm2 = new TableManager(false, tx);
        var layout = tm2.GetLayout("MyTable", tx);
        var schema2 = layout.Schema;

        Assert.Equal([.. schema2.Fields.Order()], schema.Fields.Order().ToList());
    }

    [Fact]
    public void GetLayout_new_database_create_catalog_table()
    {
        var tx = CreateTransaction();
        var tm = new TableManager(true, tx);
        tx.Commit();

        var tableCatalog = tm.GetLayout(TableManager.CATALOG_NAME_TABLE, tx);
        Assert.Equal(
            [CatalogSchema.FIELD_SLOT_SIZE, CatalogSchema.FIELD_TABLE_NAME],
            tableCatalog.Schema.Fields.Order().ToArray()
        );

        var fieldCatalog = tm.GetLayout(TableManager.CATALOG_NAME_FIELD, tx);
        Assert.Equal(
            [
                CatalogSchema.FIELD_FIELD_NAME,
                CatalogSchema.FIELD_LENGTH,
                CatalogSchema.FIELD_OFFSET,
                CatalogSchema.FIELD_TABLE_NAME,
                CatalogSchema.FIELD_TYPE,
            ],
            fieldCatalog.Schema.Fields.Order().ToArray()
        );
    }

    [Fact]
    public void GetLayout_exist_table()
    {
        {
            // 空のテーブルを作成する。トランザクションをCommitまで行い、永続化する。
            using var tx = CreateTransaction();
            var schema = CreateSchema();
            var _ = new TableManager(true, tx);
            _.CreateTable("MyTable", schema, tx);
            tx.Commit();
            tx.Dispose();
        }

        using var tx2 = CreateTransaction();
        var tm2 = new TableManager(false, tx2);
        var layout = tm2.GetLayout("MyTable", tx2);

        Assert.NotEmpty((global::System.Collections.IEnumerable)layout.Schema.Fields);
    }

    [Fact]
    public void GetLayout_not_exist_table()
    {
        var tx = CreateTransaction();
        var beforeInitialize = _directoryInfo.GetFiles().Select((info) => info.Name).ToList();

        var tm = new TableManager(true, tx);
        var layout = tm.GetLayout("NotExistTable", tx);

        Assert.Empty((global::System.Collections.IEnumerable)layout.Schema.Fields);
    }
}
