using SimpleDB.SqlParser;

namespace SimpleDB.Test.SqlParser;

public class StreamTokenizerTest
{
    private static string[] keywords =
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

    [Theory]
    [InlineData("1")]
    [InlineData("\"1\"")]
    [InlineData("'1'")]
    public void Constructor_初期値(string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new SimpleDB.SqlParser.StreamTokenizer(reader);

        Assert.Equal(-4, tokenizer.TType);
        Assert.Null(tokenizer.SVal);
        Assert.Equal(0.0, tokenizer.NVal);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("^")]
    [InlineData("=")]
    [InlineData("'")]
    public void TType_symbols(string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new SimpleDB.SqlParser.StreamTokenizer(reader);

        var actual = tokenizer.NextToken();

        Assert.Equal(s[0], actual);
        Assert.Null(tokenizer.SVal);
    }

    [Theory]
    // [InlineData('\'', "aaaa")]
    [InlineData('\'', "'aaaa'")]
    [InlineData('"', "\"aaaa\"")]
    public void TType_SVal(char c, string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new SimpleDB.SqlParser.StreamTokenizer(reader);

        var actual1 = tokenizer.NextToken();
        var actual2 = tokenizer.SVal;

        Assert.Equal(c, actual1);
        Assert.Equal("aaaa", actual2);
    }

    [Fact]
    public void TType_SVal2()
    {
        var tokenizer = new SimpleDB.SqlParser.StreamTokenizer("+aaaa+");

        Assert.Equal('+', tokenizer.NextToken());
        Assert.Null(tokenizer.SVal);
        Assert.Equal(SimpleDB.SqlParser.StreamTokenizer.TT_WORD, tokenizer.NextToken());
        Assert.Null("aaaa");
        Assert.Equal('+', tokenizer.NextToken());
        Assert.Null(tokenizer.SVal);
    }
}
