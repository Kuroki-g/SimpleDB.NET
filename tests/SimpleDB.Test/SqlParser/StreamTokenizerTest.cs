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
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("^")]
    [InlineData("=")]
    [InlineData("'")]
    public void TType_symbols(string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new StreamTokenizer(reader);

        var actual = tokenizer.NextToken();

        Assert.Equal(s[0], actual);
    }

    private static void PrintCurrentToken(StreamTokenizer tok)
    {
        if (tok.TType == StreamTokenizer.TT_NUMBER)
        {
            Console.WriteLine("IntConstant " + (int)tok.NVal);
        }
        else if (tok.TType == StreamTokenizer.TT_WORD)
        {
            string word = tok.SVal;
            if (keywords.Contains(word))
            {
                Console.WriteLine("Keyword " + word);
            }
            else
            {
                Console.WriteLine("Id " + word);
            }
        }
        else if (tok.TType == '\'')
        {
            Console.WriteLine("StringConstant " + tok.SVal);
        }
        else
        {
            Console.WriteLine("Delimiter " + (char)tok.TType);
        }
    }
}
