namespace SimpleDB.Parser.Test;

public class LexerTest
{
    [Fact]
    public void LexerTest_new()
    {
        var lex = new Lexer("");
        string x;
        int y;
        if (lex.MatchId())
        {
            x = lex.EatId();
            lex.EatDelim('=');
            y = lex.EatIntConstant();
        }
        else
        {
            y = lex.EatIntConstant();
            lex.EatDelim('=');
            x = lex.EatId();
        }
    }
}
