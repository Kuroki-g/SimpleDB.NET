using SimpleDB.Structure;

namespace SimpleDB.Metadata;

/// <summary>
/// NOTE: TableCatalogSchemaとFieldCatalogSchemaで定義したほうがよさそうだ。
/// テーブルに関する知識がTableManagerに記載されている状態が良くない。
/// </summary>
internal sealed class CatalogSchema : Schema
{
    public static readonly string FIELD_TABLE_NAME = "tblname";

    public static readonly string FIELD_FIELD_NAME = "filname";

    public static readonly string FIELD_TYPE = "type";

    public static readonly string FIELD_OFFSET = "offset";

    public static readonly string FIELD_SLOT_SIZE = "slotsize";

    public static readonly string FIELD_LENGTH = "length";
}
