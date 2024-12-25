using SimpleDB.Structure;

namespace SimpleDB.Metadata;

/// <summary>
/// NOTE: TableCatalogSchemaとFieldCatalogSchemaで定義したほうがよさそうだ。
/// テーブルに関する知識がTableManagerに記載されている状態が良くない。
/// </summary>
internal sealed class ViewSchema : Schema
{
    public static readonly string TABLE_NAME_VIEW_CATALOG = "viewcat";

    public static readonly string FIELD_VIEW_NAME = "viewname";

    public static readonly string FIELD_VIEW_DEF = "viewdef";

    public ViewSchema(int viewNameLength, int viewDefLength)
    {
        AddStringField(FIELD_VIEW_NAME, viewNameLength);
        AddStringField(FIELD_VIEW_DEF, viewDefLength);
    }

    public static void SetFields(TableScan ts, string viewName, string viewDef)
    {
        // TODO: dataのサイズが大きい場合にblockの入れ替えがうまく出来ずに無限ループしてしまう。
        ts.SetString(FIELD_VIEW_NAME, viewName);
        ts.SetString(FIELD_VIEW_DEF, viewDef);
    }
}
