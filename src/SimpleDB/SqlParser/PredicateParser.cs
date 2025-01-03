using SimpleDB.SqlParser.Grammar;

namespace SimpleDB.SqlParser;

public class PredicateParser(string s)
{
    private readonly Lexer _lexer = new(s);

    public string Field => _lexer.EatId();

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
        return _lexer.IsIdMatch ? new Expression(Field) : new Expression(Constant);
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
