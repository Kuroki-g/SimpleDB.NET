namespace SimpleDB.SqlParser.Grammar.Create;

public class CreateIndex(string indexName, string tableName, string fieldName) : ICreate
{
    public readonly string IndexName = indexName;

    public readonly string TableName = tableName;

    public readonly string FieldName = fieldName;
}
