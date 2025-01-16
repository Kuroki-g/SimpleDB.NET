using System.Text;

namespace Common;

/// <summary>
/// JavaのStreamTokenizerのAPIを元に実装したもの。
/// 実装に甘い場所があるかもしれないので、注意が必要。
/// </summary>
/// <author>Kuroki-g</author>
/// <license>MIT</license>
public class StreamTokenizer : IDisposable
{
    /// <summary>
    /// ストリームの終わりが読み込まれたことを示す。
    /// </summary>
    public readonly static int TT_EOF = -1;

    /// <summary>
    /// 行の終わりが読み込まれたことを示す。
    /// </summary>
    public readonly static int TT_EOL = 10;

    /// <summary>
    /// 数値が読み込まれたことを示す。
    /// </summary>
    public readonly static int TT_NUMBER = -2;

    /// <summary>
    /// 単語が読み込まれたことを示す。
    /// </summary>
    public readonly static int TT_WORD = -3;

    public static readonly string NEW_LINE = Environment.NewLine;

    public int LineNo { get; private set; } = 1;

    /// <summary>
    /// TT_WORDは、トークンがワードであることを示す。
    /// TT_NUMBERは、トークンが数値であることを示す。
    /// TT_EOLは、行の終わりに達したことを示す。eolIsSignificantメソッドが引数trueで呼び出された場合、このフィールドはこの値のみを持つことができる。
    /// TT_EOFは、入力ストリームの終わりに達したことを示す。
    /// </summary>
    /// <remarks>
    /// 初期値は-4
    /// </remarks>
    public int TType { get; private set; } = -4;

    /// <summary>
    /// NValのデフォルト値。リセットするときに使う。
    /// </summary>
    private static readonly double DefaultNVal = 0.0;

    /// <summary>
    /// 現在のトークンが数値の場合、その数値がここに格納される。
    /// <see cref="TType"/>がTT_NUMBERの場合でない場合、このフィールドは0に設定される。
    /// </summary>
    /// <remarks>
    /// 初期値は0.0。
    /// </remarks>
    public double NVal { get; private set; } = DefaultNVal;

    private static readonly string? DefaultSVal = null;

    /// <summary>
    /// 現在のトークンがワードの場合、その文字列がここに格納される。
    /// <see cref="TType"/>がTT_WORDの場合でない場合、このフィールドはnullに設定される。
    /// </summary>
    /// <remarks>
    /// 初期値はnull。
    /// </remarks>
    public string? SVal { get; private set; } = DefaultSVal;

    private readonly TextReader _reader;

    private readonly string[] _tokens;

    /// <summary>
    /// Javaの移植前における、定義そのままの配列である。
    /// '\u0000' 〜 '\u0020' のバイト値はすべて空白と見なす。
    /// </summary>
    public readonly static List<char> WhiteSpaces =
    [
        '\u0000',
        '\u0001',
        '\u0002',
        '\u0003',
        '\u0004',
        '\u0005',
        '\u0006',
        '\u0007',
        '\u0008',
        '\u0009',
        '\u000A',
        '\u000B',
        '\u000C',
        '\u000D',
        '\u000E',
        '\u000F',
        '\u0010',
        '\u0011',
        '\u0012',
        '\u0013',
        '\u0014',
        '\u0015',
        '\u0016',
        '\u0017',
        '\u0018',
        '\u0019',
        '\u001A',
        '\u001B',
        '\u001C',
        '\u001D',
        '\u001E',
        '\u0020',
    ];

    public static readonly char[] NonOrdinaryChars = [',', '.', ';', '!', '?'];

    public static readonly char[] Quotes = ['\'', '\"'];

    private int _currentPos = 0;

    /// <summary>
    /// JavaにおけるStreamTokenizerの実装をある程度行ったもの。
    /// 'A' 〜 'Z'、'a' 〜 'z'、および '\u00A0' 〜 '\u00FF' のバイト値はすべて英字と見なす
    /// '/' はコメント文字
    /// 単一引用 '\'' と二重引用符 ''' は文字列の引用文字
    /// 数値は構文解析される
    /// 行末記号は、独立したトークンではなく空白文字として扱う
    /// C スタイルおよび C++ スタイルのコメントは認識しない
    /// </summary>
    /// <param name="reader"></param>
    public StreamTokenizer(StringReader reader)
    {
        _reader = reader;
        // merge WhiteSpaces and NEW_LINE
        List<char> n = [.. WhiteSpaces, .. NEW_LINE.ToCharArray()];
        var targets = n.ToArray();

        var input = reader.ReadToEnd();
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';

        foreach (var c in input)
        {
            if (inQuotes)
            {
                currentToken.Append(c);
                if (c == quoteChar)
                {
                    inQuotes = false;
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                if (Quotes.Contains(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                    inQuotes = true;
                    quoteChar = c;
                    currentToken.Append(c);
                }
                else if (targets.Contains(c) || IsSymbol(c) || NonOrdinaryChars.Contains(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    currentToken.Append(c);
                }
            }
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        _tokens = tokens
            // trim spaces in "WhiteSpaces" variable
            .Select(t => t.Trim(WhiteSpaces.ToArray()))
            // filter out empty strings
            .Where(t => !string.IsNullOrEmpty(t))
            // filter spaces in "WhiteSpaces" variable
            .Where(t => !WhiteSpaces.Contains(t[0]))
            .ToArray();
    }

    /// <summary>
    /// StringReaderを呼び出してラップして使用する。
    /// </summary>
    /// <param name="s"></param>
    public StreamTokenizer(string s)
        : this(new StringReader(s)) { }

    /// <summary>
    /// TT_WORD トークンを小文字にするかどうかを 決定する。
    /// </summary>
    /// <param name="flag">trueの場合にlower caseにする。</param>
    public void LowerCaseMode(bool flag)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// このトークナイザの入力ストリームから次のトークンを読み取り、そのトークンを構文解析します。
    /// </summary>
    /// <returns>
    /// <see cref="TT_EOF" />もしくは対応する値を返す。
    /// <see cref="NextToken" />が文字列定数を検出すると、<see cref="TType" />には文字列区切り文字が設定される。
    /// この時、<see cref="SVal" />フィールドには文字列の本体が設定されます。
    /// </returns>
    public int NextToken()
    {
        (NVal, SVal, TType) = GetValuesAndTType();
        _currentPos += 1;

        return TType;
    }

    private (double NVal, string? SVal, int TType) GetValuesAndTType()
    {
        if (_currentPos >= _tokens.Length)
        {
            return (NVal = DefaultNVal, SVal = DefaultSVal, TType = TT_EOF);
        }

        var firstChar = _tokens[_currentPos][0];
        // クォーテーションの場合には、その文字列を取得する。
        if (Quotes.Contains(firstChar))
        {
            var quoteChar = firstChar;
            // 一文字しかない場合を考慮する。この場合には空文字を返す。
            if (_tokens[_currentPos].Length == 1)
            {
                return (NVal = DefaultNVal, SVal = string.Empty, TType = quoteChar);
            }
            // 両方の端が同じ引用符の場合にはトリミングし、その文字列を取得する。
            if (quoteChar == _tokens[_currentPos][^1])
            {
                return (NVal = DefaultNVal, SVal = _tokens[_currentPos][1..^1], TType = quoteChar);
            }
            // 異なる場合には始めのみトリミングし、その文字列を取得する。
            return (NVal = DefaultNVal, SVal = _tokens[_currentPos][1..], TType = quoteChar);
        }

        // 記号の場合には1文字になっているのでそのまま返す。
        if (IsSymbol(firstChar) || NonOrdinaryChars.Contains(firstChar))
        {
            return (NVal = DefaultNVal, SVal = DefaultSVal, TType = firstChar);
        }

        if (double.TryParse(_tokens[_currentPos], out double nVal))
        {
            // TT_NUMBER
            return (NVal = nVal, SVal = DefaultSVal, TType = TT_NUMBER);
        }

        if (_tokens[_currentPos] == NEW_LINE)
        {
            SVal = _tokens[_currentPos];
            return (NVal = DefaultNVal, SVal = DefaultSVal, TType = TT_EOL);
        }

        return (NVal = DefaultNVal, SVal = _tokens[_currentPos], TType = TT_WORD);
    }

    private static bool IsSymbol(char c)
    {
        return char.IsPunctuation(c) || char.IsSymbol(c);
    }

    /// <summary>
    /// 引数がこのトークナイザの「通常」文字として扱われるようにします。
    /// </summary>
    /// <param name="v"></param>
    public void OrdinaryChar(char v)
    {
        // TODO: Implement
    }

    /// <summary>
    /// 現在のストリームトークンの文字列表現と、それが発生する行番号を返します。
    /// </summary>
    /// <example>Token['a'], line 10</example>
    /// <returns></returns>
    public override string ToString()
    {
        if (TType == TT_NUMBER)
        {
            return $"Token[{(int)NVal}], line {LineNo}";
        }
        else if (TType == TT_WORD)
        {
            return $"Token[{SVal}], line {LineNo}";
        }
        else if (TType == TT_EOF)
        {
            return $"Token[EOF], line {LineNo}";
        }
        else if (TType == '\'')
        {
            return $"Token[{SVal}], line {LineNo}";
        }
        else
        {
            return $"Token[{(char)TType}], line {LineNo}";
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _reader.Dispose();
    }
}
