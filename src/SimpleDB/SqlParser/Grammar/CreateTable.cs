using SimpleDB.Structure;

namespace SimpleDB.SqlParser.Grammar;

public class CreateTable(Schema schema, string tableName)
{
    public readonly Schema NewSchema = schema;

    public readonly string TableName = tableName;
}
