namespace SimpleDB.SqlParser;

public class Lexer : IDisposable
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
    private readonly StreamTokenizer _tokenizer;

    public Lexer(string s)
    {
        _tokenizer = new StreamTokenizer(s);
        NextToken();
    }

    private void NextToken()
    {
        try
        {
            _tokenizer.NextToken();
        }
        catch (Exception e)
        {
            throw new BadSyntaxException(e.Message);
        }
    }

    /// <summary>
    /// TODO: ほんとうにこのキャストで正しいかを確認する。
    /// </summary>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public bool MatchDelimiter(char delimiter) => delimiter == (char)_tokenizer.TType;

    public bool MatchIntConstant() => _tokenizer.TType == StreamTokenizer.TT_NUMBER;

    public bool MatchStringConstant() => '\'' == (char)_tokenizer.TType;

    public bool MatchKeyword(string w) =>
        _tokenizer.TType == StreamTokenizer.TT_WORD
        && w.Equals(_tokenizer.SVal, StringComparison.OrdinalIgnoreCase);

    public bool IsIdentifierMatch
    {
        get
        {
            if (_tokenizer.TType == StreamTokenizer.TT_WORD)
            {
                return !_keywords.Contains(_tokenizer.SVal, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }
    }

    public void EatDelim(char c)
    {
        if (MatchDelimiter(c))
        {
            NextToken();
            return;
        }
        throw new BadSyntaxException($"Expected delimiter {c}");
    }

    public int EatIntConstant()
    {
        if (MatchIntConstant())
        {
            var i = (int)_tokenizer.NVal;
            NextToken();
            return i;
        }
        throw new BadSyntaxException("Expected integer constant");
    }

    public string EatStringConstant()
    {
        if (MatchStringConstant())
        {
            var s = _tokenizer.SVal ?? string.Empty;
            NextToken();
            return s;
        }
        throw new BadSyntaxException("Expected string constant");
    }

    public void EatKeyword(string w)
    {
        if (MatchKeyword(w))
        {
            NextToken();
            return;
        }
        throw new BadSyntaxException($"Expected keyword {w}");
    }

    public string EatIdentifier()
    {
        if (IsIdentifierMatch)
        {
            var s = _tokenizer.SVal ?? string.Empty;
            NextToken();
            return s;
        }
        throw new BadSyntaxException("Expected identifier");
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
}
