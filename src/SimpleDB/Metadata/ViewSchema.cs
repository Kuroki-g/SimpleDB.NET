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
}
