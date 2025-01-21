namespace SimpleDB.SqlParser.Grammar.UpdateCmd;

public sealed class Modify(string table, string field, Expression newValue, Predicate predicate)
{
    public readonly string TableName = table;

    public readonly string TargetField = field;

    public readonly Expression NewValue = newValue;

    public readonly Predicate Predicate = predicate;
}
