using SimpleDB.SqlParser;

namespace SimpleDB.Test.SqlParser;

public class LexerTest
{
    // [Theory]
    // [InlineData("invalid-word")]
    // public void LexerTest_EatIdentifier_throws_exception(string input)
    // {
    //     var lex = new Lexer(input);

    //     var actual = Record.Exception(() => lex.EatIdentifier());

    //     Assert.IsType<BadSyntaxException>(actual);
    // }

    [Fact]
    public void EatIntConstant()
    {
        var lex = new Lexer("1");

        var actual = lex.EatIntConstant();

        Assert.Equal(1, actual);
    }

    [Theory]
    [InlineData("'1'")]
    [InlineData("\"1\"")]
    public void EatStringConstant(string input)
    {
        var lex = new Lexer(input);

        var actual = lex.EatStringConstant();

        Assert.Equal("1", actual);
    }

    [Fact]
    public void MatchKeyword_assert()
    {
        var lex = new Lexer("select * from table where a = 1");

        var actual = lex.MatchKeyword("select");

        Assert.True(actual);
    }
}
