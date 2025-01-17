using SimpleDB.SqlParser.Grammar.UpdateCmd;
using SimpleDB.Structure;

namespace SimpleDB.SqlParser.Grammar;

public interface ICreate : IUpdateCmd { }

public class CreateTable(Schema schema, string tableName) : ICreate
{
    public readonly Schema NewSchema = schema;

    public readonly string TableName = tableName;
}
