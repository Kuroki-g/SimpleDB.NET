using SimpleDB.Structure;

namespace SimpleDB.Metadata;

internal sealed class IndexCatalogSchema : Schema
{
    public static readonly string TABLE_NAME_IDX_CATALOG = "idxcat";

    public static readonly string FIELD_INDEX_NAME = "indexname";

    public static readonly string FIELD_INDEX_TYPE = "indextype";

    public static readonly string FIELD_TABLE_NAME = "tablename";

    public static readonly string FIELD_FIELD_NAME = "fieldname";

    public IndexCatalogSchema(int indexNameLength, int tableNameLength, int fieldNameLength)
    {
        AddStringField(FIELD_INDEX_NAME, indexNameLength);
        AddIntField(FIELD_INDEX_TYPE);
        AddStringField(FIELD_TABLE_NAME, tableNameLength);
        AddStringField(FIELD_FIELD_NAME, fieldNameLength);
    }
}
