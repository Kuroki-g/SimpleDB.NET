using System.Collections.ObjectModel;

namespace SimpleDB.SqlParser.Grammar.UpdateCmd;

public sealed class Insert(string table, List<string> fields, List<Constant> values) : IUpdateCmd
{
    public readonly string Table = table;

    public readonly ReadOnlyCollection<string> Fields = new(fields);

    public readonly ReadOnlyCollection<Constant> Values = new(values);
}
