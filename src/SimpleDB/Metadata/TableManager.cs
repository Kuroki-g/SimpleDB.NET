using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public class TableManager : ITableManager
{
    public readonly int MAX_NAME = 16;

    public static readonly string CATALOG_NAME_TABLE = "tblcat";

    public static readonly string CATALOG_NAME_FIELD = "fldcat";

    private readonly Layout _tCatLayout,
        _fCatLayout;

    /// <summary>
    /// 新しいテーブルマネージャーを作成する。
    /// </summary>
    /// <param name="isNew">
    /// データベースを新規作成する場合にtrueを指定する。
    /// 指定すると、新規にカタログテーブルが作成される。
    /// </param>
    /// <param name="tx"></param>
    public TableManager(bool isNew, ITransaction tx)
    {
        var tCatSchema = new CatalogSchema();
        tCatSchema.AddStringField(CatalogSchema.FIELD_TABLE_NAME, MAX_NAME);
        tCatSchema.AddIntField(CatalogSchema.FIELD_SLOT_SIZE);
        _tCatLayout = new Layout(tCatSchema);

        var fCatSchema = new CatalogSchema();
        fCatSchema.AddStringField(CatalogSchema.FIELD_TABLE_NAME, MAX_NAME);
        fCatSchema.AddStringField(CatalogSchema.FIELD_FIELD_NAME, MAX_NAME);
        fCatSchema.AddIntField(CatalogSchema.FIELD_TYPE);
        fCatSchema.AddIntField(CatalogSchema.FIELD_LENGTH);
        fCatSchema.AddIntField(CatalogSchema.FIELD_OFFSET);
        _fCatLayout = new Layout(fCatSchema);

        if (isNew)
        {
            CreateTable(CATALOG_NAME_TABLE, tCatSchema, tx);
            CreateTable(CATALOG_NAME_FIELD, fCatSchema, tx);
        }
    }

    /// <summary>
    /// 新しいテーブルを作成する。
    /// 指定の名前とスキーマを持つ。
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="schema"></param>
    /// <param name="tx"></param>
    public void CreateTable(string tableName, ISchema schema, ITransaction tx)
    {
        var layout = new Layout(schema);
        // テーブルカタログに作成するテーブルの名称を記録する。
        var tCat = new TableScan(tx, CATALOG_NAME_TABLE, _tCatLayout);
        tCat.Insert();
        tCat.SetString(CatalogSchema.FIELD_TABLE_NAME, tableName);
        tCat.SetInt(CatalogSchema.FIELD_SLOT_SIZE, layout.SlotSize);
        tCat.Close();

        // 追加するフィールドをフィールドカタログに記録する。
        var fCat = new TableScan(tx, CATALOG_NAME_FIELD, _fCatLayout);
        foreach (var fieldName in schema.Fields)
        {
            fCat.Insert();
            fCat.SetString(CatalogSchema.FIELD_TABLE_NAME, tableName);
            fCat.SetString(CatalogSchema.FIELD_FIELD_NAME, fieldName);
            fCat.SetInt(CatalogSchema.FIELD_TYPE, schema.Type(fieldName));
            fCat.SetInt(CatalogSchema.FIELD_LENGTH, schema.Length(fieldName));
            fCat.SetInt(CatalogSchema.FIELD_OFFSET, layout.Offset(fieldName));
        }
        fCat.Close();
    }

    public Layout GetLayout(string tableName, ITransaction tx)
    {
        var size = -1;
        var tCat = new TableScan(tx, CATALOG_NAME_TABLE, _tCatLayout);
        while (tCat.Next())
        {
            var isEqual = tCat.GetString(CatalogSchema.FIELD_TABLE_NAME).Equals(tableName);
            if (isEqual)
            {
                size = tCat.GetInt(CatalogSchema.FIELD_SLOT_SIZE);
                break;
            }
        }
        tCat.Close();

        var schema = new Schema();
        var offsets = new Dictionary<string, int>(); // TODO: ただ保持するだけでDictionaryはコストが高い
        var fCat = new TableScan(tx, CATALOG_NAME_FIELD, _fCatLayout);
        while (fCat.Next())
        {
            var isEqual = fCat.GetString(CatalogSchema.FIELD_TABLE_NAME).Equals(tableName);
            if (isEqual)
            {
                // NOTE: ここが変更に弱いので、catalogは別のクラスにしてしまいたい。
                var fieldName = fCat.GetString(CatalogSchema.FIELD_FIELD_NAME);
                var fieldType = fCat.GetInt(CatalogSchema.FIELD_TYPE);
                var fieldLength = fCat.GetInt(CatalogSchema.FIELD_LENGTH);
                var offset = fCat.GetInt(CatalogSchema.FIELD_OFFSET);
                offsets[fieldName] = offset;
                schema.AddField(fieldName, fieldType, fieldLength);
            }
        }
        fCat.Close();

        return new Layout(schema, offsets, size);
    }
}
