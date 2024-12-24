using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Metadata;

internal class IndexSchema : Schema
{
    public static readonly string FIELD_ID = "id";

    public static readonly string FIELD_BLOCK = "block";

    public static readonly string FIELD_DATA_VALUE = "dataval";

    public IndexSchema(string fieldName)
    {
        var info = GetFieldInfo(fieldName);
        AddIntField(FIELD_BLOCK);
        AddIntField(FIELD_ID);

        if (info.Type == Types.INTEGER)
        {
            AddIntField(FIELD_DATA_VALUE);
        }
        else
        {
            AddStringField(FIELD_DATA_VALUE, info.Length);
        }
    }
}
