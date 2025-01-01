using SimpleDB.Parser;

namespace SimpleDB.Test.Parser;

public class LexerTest
{
    [Fact]
    public void LexerTest_new()
    {
        var lex = new Lexer("");
        string x;
        int y;
        if (lex.IsIdMatch)
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
