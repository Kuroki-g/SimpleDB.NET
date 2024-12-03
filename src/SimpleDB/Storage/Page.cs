using System.Text;

namespace SimpleDB.Storage;

public sealed class Page
{
    private readonly MemoryStream _stream;

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Encoding CHARSET = Encoding.ASCII;
#pragma warning restore CA2211 // Non-constant fields should not be visible

    /// <summary>
    /// A constructor for creating data buffers
    /// </summary>
    /// <param name="blockSize"></param>
    public Page(int blockSize)
    {
        _stream = new MemoryStream(blockSize);
    }

    /// <summary>
    /// A constructor for creating log pages
    /// </summary>
    /// <param name="bytes"></param>
    public Page(byte[] bytes)
    {
        _stream = new MemoryStream(bytes);
    }

    /// <summary>
    /// WARNING: あまりよく分かっていない。移植が間違っているはず。
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public long GetInt(int offset)
    {
        return _stream.Position;
    }

    /// <summary>
    /// WARNING: あまりよく分かっていない。移植が間違っているはず。
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="n"></param>
    public void SetInt(int offset, int n)
    {
        _stream.Position = offset + n;
    }

    /// <summary>
    /// TODO: Bufferの開始位置があっているか確認する。
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public byte[] GetBytes(int offset)
    {
        _stream.Position = offset;
        return _stream.GetBuffer();
    }


    public void SetBytes(int offset, byte[] bytes)
    {
        _stream.Position = offset;
        foreach (byte b in bytes)
        {
            _stream.WriteByte(b);
            _stream.Position += 1;
        }
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

    public static int MaxLength(int strlen)
    {
        var bytesPerChar = CHARSET.GetMaxByteCount(strlen);
        // WARNING: return Integer.BYTES + (strlen * (int)bytesPerChar); だったが、Integer.BYTESがよく分かっていない。
        return strlen * (int)bytesPerChar;
    }

    internal byte[] Contents()
    {
        _stream.Position = 0;
        return _stream.GetBuffer();
    }
}