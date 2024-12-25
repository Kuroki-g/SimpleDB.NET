using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

internal class TableManager : ITableManager
{
    public int MAX_NAME { get; } = 16;

    /// <summary>
    /// TODO: 恐らくここにあるよりは新しく概念として定義した方がよさそうだ。
    /// </summary>
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
        var tCatScan = new TableScan(tx, CATALOG_NAME_TABLE, _tCatLayout);
        tCatScan.Insert();
        tCatScan.SetString(CatalogSchema.FIELD_TABLE_NAME, tableName);
        tCatScan.SetInt(CatalogSchema.FIELD_SLOT_SIZE, layout.SlotSize);
        tCatScan.Close();

        // 追加するフィールドをフィールドカタログに記録する。
        var fCatScan = new TableScan(tx, CATALOG_NAME_FIELD, _fCatLayout);
        foreach (var fieldName in schema.Fields)
        {
            fCatScan.Insert();
            fCatScan.SetString(CatalogSchema.FIELD_TABLE_NAME, tableName);
            fCatScan.SetString(CatalogSchema.FIELD_FIELD_NAME, fieldName);
            fCatScan.SetInt(CatalogSchema.FIELD_TYPE, schema.Type(fieldName));
            fCatScan.SetInt(CatalogSchema.FIELD_LENGTH, schema.Length(fieldName));
            fCatScan.SetInt(CatalogSchema.FIELD_OFFSET, layout.Offset(fieldName));
        }
        fCatScan.Close();
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
