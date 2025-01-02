namespace SimpleDB.Parser.Grammar;

/// <summary>
/// Represents a query.
/// </summary>
/// <see href="https://github.com/Kuroki-g/SimpleDB.NET/blob/main/documents/Parser/Parser.md" />
public class Query(List<string> fields, IEnumerable<string> tables, Predicate predicate)
{
    public readonly List<string> Fields = fields;

    public readonly IEnumerable<string> Tables = tables;

    public readonly Predicate Predicate = predicate;

    private readonly string _template = "select {SelectList} from {TableList}";

    private readonly string _where = " where {Predicate}";

    public override string ToString()
    {
        var selectList = string.Join(", ", Fields);
        var tableList = string.Join(", ", Tables);
        var predicate = Predicate.ToString();
        return predicate == string.Empty
            ? string.Format(_template, selectList, tableList)
            : string.Format(_template + _where, selectList, tableList, predicate);
    }
}
