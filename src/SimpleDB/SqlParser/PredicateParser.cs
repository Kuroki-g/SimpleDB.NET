using SimpleDB.SqlParser.Grammar;

namespace SimpleDB.SqlParser;

public class PredicateParser(string s)
{
    private readonly Lexer _lexer = new(s);

    public string Field()
    {
        return _lexer.EatIdentifier();
    }

    public void Constant()
    {
        if (_lexer.MatchStringConstant())
        {
            _lexer.EatStringConstant();
        }
        else
        {
            _lexer.EatIntConstant();
        }
    }

    public void Expression()
    {
        if (_lexer.IsIdentifierMatch)
        {
            Field();
        }
        else
        {
            Constant();
        }
    }

    /// <summary>
    /// See: documentation to know about details
    /// </summary>
    public void Term()
    {
        Expression();
        _lexer.EatDelim('=');
        Expression();
    }

    /// <summary>
    /// See: documentation to know about details
    /// </summary>
    public void Predicate()
    {
        Term();
        while (_lexer.MatchKeyword("and"))
        {
            _lexer.EatKeyword("and");
            Term();
        }
    }
}
