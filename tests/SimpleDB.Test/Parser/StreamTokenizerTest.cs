using SimpleDB.Parser;

namespace SimpleDB.Test.Parser;

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

    [Fact]
    public void Test1()
    {
        var s = "()"; // change later
        var reader = new StringReader(s);
        var tok = new StreamTokenizer(reader);
        tok.OrdinaryChar('.');
        // tok.LowerCaseMode(true); //ids and keywords are converted to lower case
        while (tok.NextToken() != StreamTokenizer.TT_EOF)
        {
            PrintCurrentToken(tok);
        }
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
