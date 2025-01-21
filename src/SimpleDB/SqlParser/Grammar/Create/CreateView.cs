namespace SimpleDB.SqlParser.Grammar.Create;

public class CreateView(string viewName, Query query) : ICreate
{
    public readonly string ViewName = viewName;

    public string ViewDef => _query.ToString();

    private readonly Query _query = query;
}
