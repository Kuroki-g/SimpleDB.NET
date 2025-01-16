using SimpleDB.SqlParser;

namespace SimpleDB.Test.SqlParser;

public class LexerTest
{
    [Theory]
    [InlineData("select")]
    [InlineData("from")]
    [InlineData("where")]
    public void LexerTest_EatIdentifier_throws_exception(string input)
    {
        var lex = new Lexer(input);

        var actual = Record.Exception(() => lex.EatIdentifier());

        Assert.IsType<BadSyntaxException>(actual);
    }

    [Fact]
    public void EatIntConstant()
    {
        var lex = new Lexer("1");

        var actual = lex.EatIntConstant();

        Assert.Equal(1, actual);
    }

    [Fact]
    public void EatStringConstant()
    {
        var lex = new Lexer("'1'");

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

    [Fact]
    public void MatchDelimiter_assert()
    {
        var lex = new Lexer(",");

        var actual = lex.MatchDelimiter(',');

        Assert.True(actual);
    }

    [Fact]
    public void MatchIntConstant_assert()
    {
        var lex = new Lexer("123");

        var actual = lex.MatchIntConstant();

        Assert.True(actual);
    }

    [Theory]
    [InlineData("identifier", true)]
    [InlineData("anotherIdentifier", true)]
    [InlineData("select", false)]
    [InlineData("from", false)]
    public void IsIdentifierMatch_assert(string input, bool expected)
    {
        var lex = new Lexer(input);

        var actual = lex.MatchIdentifier();

        Assert.Equal(expected, actual);
    }
}
