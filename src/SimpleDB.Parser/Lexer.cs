namespace SimpleDB.Parser;

public class Lexer
{
    private readonly string[] _keywords =
    [
        "select",
        "from",
        "where",
        "and",
        "insert",
        "into",
        "values",
        "delete",
        "update",
        "set",
        "create",
        "table",
        "int",
        "varchar",
        "view",
        "as",
        "index",
        "on",
    ];

    public Lexer(string s) { }

    public bool MatchDelim(char c)
    {
        throw new NotImplementedException();
    }

    public bool MatchIntConstant(char c)
    {
        throw new NotImplementedException();
    }

    public bool MatchIntConstant()
    {
        throw new NotImplementedException();
    }

    public bool MatchKeyword(string w)
    {
        throw new NotImplementedException();
    }

    public bool MatchId()
    {
        throw new NotImplementedException();
    }

    public void EatDelim(char c)
    {
        throw new NotImplementedException();
    }

    public int EatIntConstant()
    {
        throw new NotImplementedException();
    }

    public void EatKeyword(string w)
    {
        throw new NotImplementedException();
    }

    public string EatId()
    {
        throw new NotImplementedException();
    }
}
