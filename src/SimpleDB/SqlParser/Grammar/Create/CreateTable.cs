using SimpleDB.Structure;

namespace SimpleDB.SqlParser.Grammar.Create;

public class CreateTable(Schema schema, string tableName) : ICreate
{
    public readonly Schema NewSchema = schema;

    public readonly string TableName = tableName;
}
