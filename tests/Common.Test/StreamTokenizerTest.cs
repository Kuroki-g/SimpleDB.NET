namespace Common.Test;

public class StreamTokenizerTest
{
    class TokenizerElement
    {
        public int TType { get; init; }
        public string? SVal { get; init; } = null;

        public double NVal { get; init; } = 0.0;
    }

    [Fact]
    public void 数字の1をトークナイズできる()
    {
        var reader = new StringReader("1");
        var tokenizer = new StreamTokenizer(reader);
        var result = StreamTokenizerPrintln(tokenizer);

        List<TokenizerElement> expected =
        [
            new TokenizerElement
            {
                TType = StreamTokenizer.TT_NUMBER,
                SVal = null,
                NVal = 1.0,
            },
            new TokenizerElement { TType = StreamTokenizer.TT_EOF },
        ];

        AssertTokens(expected, result);
    }

    [Fact]
    public void ダブルクォートで囲まれた1をトークナイズできる()
    {
        var reader = new StringReader("\"1\"");
        var tokenizer = new StreamTokenizer(reader);
        var result = StreamTokenizerPrintln(tokenizer);

        List<TokenizerElement> expected =
        [
            new TokenizerElement { TType = '\"', SVal = "1" },
            new TokenizerElement { TType = StreamTokenizer.TT_EOF },
        ];

        AssertTokens(expected, result);
    }

    [Fact]
    public void シングルクォートで囲まれた1をトークナイズできる()
    {
        var reader = new StringReader("'1'");
        var tokenizer = new StreamTokenizer(reader);

        var result = StreamTokenizerPrintln(tokenizer);

        List<TokenizerElement> expected =
        [
            new TokenizerElement { TType = '\'', SVal = "1" },
            new TokenizerElement { TType = StreamTokenizer.TT_EOF },
        ];

        AssertTokens(expected, result);
    }

    [Fact]
    public void シングルクォートとダブルクォーテーションで囲まれた1をトークナイズできる()
    {
        var reader = new StringReader("'1\" ");
        var tokenizer = new StreamTokenizer(reader);

        var result = StreamTokenizerPrintln(tokenizer);

        List<TokenizerElement> expected =
        [
            new TokenizerElement { TType = '\'', SVal = "1\"" },
            new TokenizerElement { TType = StreamTokenizer.TT_EOF },
        ];

        AssertTokens(expected, result);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("^")]
    [InlineData("=")]
    public void TType_symbols(string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new StreamTokenizer(reader);

        var actual = tokenizer.NextToken();

        Assert.Equal(s[0], actual);
        Assert.Null(tokenizer.SVal);
    }

    [Theory]
    [InlineData("'")]
    [InlineData("\"")]
    public void TType_quote(string s)
    {
        var reader = new StringReader(s);
        var tokenizer = new StreamTokenizer(reader);

        var actual = tokenizer.NextToken();

        Assert.Equal(s[0], actual);
        Assert.Equal(string.Empty, tokenizer.SVal);
    }

    [Fact]
    public void ある程度の長さを持った文をトークナイズできる()
    {
        var reader = new StringReader("select field1, field2 from table1 where field1 = 'value1'");
        var tokenizer = new StreamTokenizer(reader);

        var actual = StreamTokenizerPrintln(tokenizer);

        List<TokenizerElement> expected =
        [
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "select" },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "field1" },
            new TokenizerElement { TType = ',', SVal = null },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "field2" },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "from" },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "table1" },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "where" },
            new TokenizerElement { TType = StreamTokenizer.TT_WORD, SVal = "field1" },
            new TokenizerElement { TType = '=', SVal = null },
            new TokenizerElement { TType = '\'', SVal = "value1" },
            new TokenizerElement { TType = StreamTokenizer.TT_EOF },
        ];

        AssertTokens(expected, actual);
    }

    private static void AssertTokens(List<TokenizerElement> expected, List<TokenizerElement> actual)
    {
        // cast all using linq and assert as list
        var e = expected
            .Select(x => $"TType: {x.TType} SVal: {x.SVal ?? "null"} NVal: {x.NVal}")
            .ToList();
        var a = actual
            .Select(x => $"TType: {x.TType} SVal: {x.SVal ?? "null"} NVal: {x.NVal}")
            .ToList();

        Assert.Equal(e, a);
    }

    private static List<TokenizerElement> StreamTokenizerPrintln(StreamTokenizer tokenizer)
    {
        List<TokenizerElement> result = [];
        while (tokenizer.TType != StreamTokenizer.TT_EOF)
        {
            tokenizer.NextToken();
            result.Add(
                new TokenizerElement
                {
                    TType = tokenizer.TType,
                    SVal = tokenizer.SVal,
                    NVal = tokenizer.NVal,
                }
            );
        }

        return result;
    }
}
