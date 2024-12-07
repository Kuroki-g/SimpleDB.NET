using System.Text;
using Common;

namespace SimpleDB.Storage;

public sealed class Page : IDisposable
{
    private readonly MemoryStream _stream;

    private readonly BinaryWriter _writer;

    private readonly BinaryReader _reader;

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Encoding CHARSET = Encoding.UTF8; // UTF-32のほうがいいかもしれない。
#pragma warning restore CA2211 // Non-constant fields should not be visible

    /// <summary>
    /// A constructor for creating data buffers
    /// </summary>
    /// <param name="blockSize"></param>
    public Page(int blockSize)
    {
        _stream = new MemoryStream(new byte[blockSize]);
        _writer = new BinaryWriter(_stream, CHARSET);
        _reader = new BinaryReader(_stream, CHARSET);
    }

    /// <summary>
    /// A constructor for creating log pages
    /// </summary>
    /// <param name="bytes"></param>
    public Page(byte[] bytes)
    {
        _stream = new MemoryStream(bytes);
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);
    }

    /// <summary>
    /// WARNING: あまりよく分かっていない。移植が間違っているはず。
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int GetInt(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        return _reader.ReadInt32();
    }

    /// <summary>
    /// WARNING: あまりよく分かっていない。移植が間違っているはず。
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="n"></param>
    public void SetInt(int offset, int n)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _writer.Write(n);
    }

    /// <summary>
    /// TODO: Bufferの開始位置があっているか確認する。
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public byte[] GetBytes(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        try
        {
            int length = _reader.ReadInt32();
            var bytes = _reader.ReadBytes(length);
            return bytes;
        }
        catch (EndOfStreamException)
        {
            return [];
        }
    }

    /// <summary>
    /// WARNING: 元々の実装がバッファオーバーフローしてしまうはずなので、現在の実装だと想定通りにならないはずである。
    /// ページのサイズより大きいデータが割り当てられた時の例外処理がなされていない。
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="bytes"></param>
    public void SetBytes(int offset, byte[] bytes)
    {
        _stream.Position = offset;
        _writer.Write(bytes.Length);
        _writer.Write(bytes);
    }

    public string GetString(int offset)
    {
        byte[] bytes = GetBytes(offset);
        return CHARSET.GetString(bytes);
    }

    public void SetString(int offset, string s)
    {
        byte[] bytes = CHARSET.GetBytes(s);
        SetBytes(offset, bytes);
    }

    /// <summary>
    /// WARNING: C#の実装ではGetMaxByteCountは文字数に比例しない。なので概算になる。
    /// <see href="https://learn.microsoft.com/ja-jp/dotnet/api/system.text.encoding.getmaxbytecount?view=net-8.0"/>
    /// </summary>
    /// <param name="strlen"></param>
    /// <returns></returns>
    public static int MaxLength(int strlen)
    {
        var bytesPerChar = CHARSET.GetMaxByteCount(1);
        return Bytes.Integer + strlen * bytesPerChar;
    }

    /// <summary>
    /// 元々の実装はByteBufferをcontents()で取得出来ていた。
    /// そのまま参照を渡すのは問題があるので書き込むようにしている。
    /// </summary>
    /// <param name="buffer">RandomAccess.Readで取得した配列</param>
    internal void SetContents(Span<byte> buffer)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.Write(buffer);
        // for debug
        var contents = _stream.ToArray();
        //
    }

    /// <summary>
    /// bufferの中身を取得する。
    /// </summary>
    /// <see href="https://learn.microsoft.com/ja-jp/dotnet/api/system.io.memorystream.-ctor?view=net-9.0"/>
    /// <returns></returns>
    internal byte[] Contents()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        var contents = _stream.ToArray();
        return contents;
    }

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
        _stream.Close();
    }
}
