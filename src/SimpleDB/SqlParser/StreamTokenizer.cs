namespace SimpleDB.SqlParser;

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
    /// 現在のトークンが数値の場合、その数値がここに格納される。
    /// <see cref="TType"/>がTT_NUMBERの場合でない場合、このフィールドは0に設定される。
    /// </summary>
    /// <remarks>
    /// 初期値は0.0。
    /// </remarks>
    public double NVal { get; private set; } = 0.0;

    /// <summary>
    /// 現在のトークンがワードの場合、その文字列がここに格納される。
    /// <see cref="TType"/>がTT_WORDの場合でない場合、このフィールドはnullに設定される。
    /// </summary>
    /// <remarks>
    /// 初期値はnull。
    /// </remarks>
    public string? SVal { get; private set; } = null;

    private TextReader _reader { get; set; }

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

    private int _currentPos = 0;

    /// <summary>
    /// Javaでは推奨されておらず、変わりにStringのsplitメソッドまたはjava.util.regexパッケージが推奨されている。
    /// 速度面では、StringTokenizerの方が有利らしく、内部の実装が異なるものだと思われる。
    /// C#では、StringTokenizerが存在しないため、内部的には正規表現を用いたものとしている。
    /// 'A' 〜 'Z'、'a' 〜 'z'、および '\u00A0' 〜 '\u00FF' のバイト値はすべて英字と見なす
    /// '/' はコメント文字
    /// 単一引用 '\'' と二重引用符 ''' は文字列の引用文字
    /// 数値は構文解析される
    /// 行末記号は、独立したトークンではなく空白文字として扱う
    /// C スタイルおよび C++ スタイルのコメントは認識しない
    /// </summary>
    /// <param name="reader"></param>
    public StreamTokenizer(TextReader reader)
    {
        _reader = reader;
        // merge WhiteSpaces and NEW_LINE
        List<char> n = [.. WhiteSpaces];
        var targets = WhiteSpaces.Concat(n).ToArray();
        _tokens = reader.ReadToEnd().Split(targets);
    }

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
        int tType = GetTTypeAndSetValue();
        _currentPos += 1;
        TType = tType;

        return tType;
    }

    private int GetTTypeAndSetValue()
    {
        if (_currentPos >= _tokens.Length)
        {
            SVal = null;
            NVal = 0.0;
            return TT_EOF;
        }
        else if (IsSymbol(_tokens[_currentPos][0]))
        {
            SVal = null;
            return _tokens[_currentPos][0];
        }
        else if (double.TryParse(_tokens[_currentPos], out double nVal))
        {
            NVal = nVal;
            return TT_NUMBER;
        }
        else if (_tokens[_currentPos] == NEW_LINE)
        {
            SVal = _tokens[_currentPos];
            return TT_EOL;
        }
        else
        {
            SVal = _tokens[_currentPos];
            return TT_WORD;
        }
    }

    public bool IsSymbol(char c)
    {
        // this is not works well because of the char.IsSymbol method.
        // return char.IsSymbol(c);
        // give me how to check the char is symbol or not like java code.
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
