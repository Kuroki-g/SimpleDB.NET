namespace SimpleDB.SqlParser.Grammar.UpdateCmd;

public sealed class Delete(string table, Predicate predicate) : IUpdateCmd
{
    public readonly string Table = table;

    public readonly Predicate Predicate = predicate;
}
