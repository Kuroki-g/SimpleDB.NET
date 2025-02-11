using System.Text;
using Common;

namespace SimpleDB.Storage;

public sealed class Page : IDisposable
{
    private readonly MemoryStream _stream;

    internal byte[] RawData => _stream.ToArray();

    private readonly BinaryWriter _writer;

    private readonly BinaryReader _reader;

    internal bool IsDisposed = false;

    public static readonly Encoding CHARSET = Encoding.UTF8;

    /// <summary>
    /// 1文字あたり必要な最大のバイト数
    /// WARNING: C#の実装ではGetMaxByteCountは文字数に比例しない。なので概算になる。
    /// </summary>
    public static readonly int MaxBytesPerChar = CHARSET.GetMaxByteCount(1);

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
        _writer = new BinaryWriter(_stream, CHARSET);
        _reader = new BinaryReader(_stream, CHARSET);
    }

    /// <summary>
    /// Get an integer from the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <returns>The integer value at the specified offset</returns>
    public int GetInt(int offset)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }
        if (offset + sizeof(int) > _stream.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                "Offset exceeds the remaining size of the stream."
            );
        }

        _stream.Seek(offset, SeekOrigin.Begin);
        byte[] bytes = _reader.ReadBytes(Bytes.Integer);
        if (BitConverter.IsLittleEndian) // JavaのByteBufferはビッグエンディアンなので
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Write an integer to the page at the specified offset.
    /// </summary>
    /// <param name="offset">The byte offset within the page</param>
    /// <param name="n">The integer value to write</param>
    public void SetInt(int offset, int n)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }

        _stream.Seek(offset, SeekOrigin.Begin);
        byte[] bytes = BitConverter.GetBytes(n);
        if (BitConverter.IsLittleEndian) // JavaのByteBufferはビッグエンディアンなので
        {
            Array.Reverse(bytes);
        }
        _writer.Write(bytes);
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
            // lengthが負の値の場合のチェック
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(
                    length.ToString(),
                    "Length cannot be negative."
                );
            }

            // lengthがストリームの残りのサイズを超える場合のチェック
            return offset + Bytes.Integer + length > _stream.Length
                ? throw new ArgumentOutOfRangeException(
                    length.ToString(),
                    "Length exceeds the remaining size of the stream."
                )
                : _reader.ReadBytes(length);
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
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }

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
        var bytes = CHARSET.GetBytes(s);
        SetBytes(offset, bytes);
    }

    /// <summary>
    /// Calculate the maximum number of bytes required to store a string of the specified length.
    /// </summary>
    /// <param name="strlen">The length of the string</param>
    /// <returns>The maximum number of bytes required</returns>
    public static int MaxLength(int strlen)
    {
        // 長さ情報(int)のための4バイト + 文字列の最大バイト数
        return Bytes.Integer + strlen * MaxBytesPerChar;
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
