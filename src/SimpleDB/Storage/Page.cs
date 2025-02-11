using System.Text;
using Common;

namespace SimpleDB.Storage;

public sealed class Page : IDisposable
{
    private readonly MemoryStream _stream;

    private readonly BinaryWriter _writer;

    private readonly BinaryReader _reader;

    internal bool IsDisposed = false;

    private static readonly Encoding CHARSET = Encoding.UTF8;

    /// <summary>
    /// 1文字あたり必要な最大のバイト数
    /// WARNING: C#の実装ではGetMaxByteCountは文字数に比例しない。なので概算になる。
    /// </summary>
    private static readonly int MaxBytesPerChar = CHARSET.GetMaxByteCount(1);

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
    /// Get an integer from the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <returns>The integer value at the specified offset</returns>
    public int GetInt(int offset)
    {
        var _ = offset >= 0 ? offset : 0;
        if (_ >= _stream.Length)
        {
            throw new ArgumentOutOfRangeException(offset.ToString());
        }

        _stream.Seek(_, SeekOrigin.Begin);
        return _reader.ReadInt32();
    }

    /// <summary>
    /// Write an integer to the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <param name="n">The integer value to write</param>
    public void SetInt(int offset, int n)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _writer.Write(n);
    }

    /// <summary>
    /// Get a byte array from the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <returns>The byte array at the specified offset</returns>
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
    /// Write a byte array to the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <param name="bytes">The byte array to write</param>
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
    /// Calculate the maximum number of bytes required to store a string of the specified length.
    /// </summary>
    /// <param name="strlen">The length of the string</param>
    /// <returns>The maximum number of bytes required</returns>
    public static int MaxLength(int strlen)
    {
        var bytesPerChar = CHARSET.GetMaxByteCount(1);
        return Bytes.Integer + strlen * bytesPerChar;
    }

    /// <summary>
    ///  Write the contents of a byte array to the page.
    /// </summary>
    /// <param name="buffer"></param>
    internal void SetContents(Span<byte> buffer)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.Write(buffer);
    }

    /// <summary>
    /// Get the contents of the page as a byte array.
    /// </summary>
    /// <returns>A byte array containing the contents of the page</returns>
    internal byte[] Contents()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        return _stream.ToArray();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                // マネージドリソースの解放
                _writer.Dispose();
                _reader.Dispose();
                _stream.Dispose();
            }

            IsDisposed = true;
        }
    }

    ~Page()
    {
        Dispose(false);
    }
}
