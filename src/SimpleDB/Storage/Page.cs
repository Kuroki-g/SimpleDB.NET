using System.Text;
using Common;

namespace SimpleDB.Storage;

public sealed class Page : IDisposable
{
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;
    internal bool IsDisposed = false;

    public static Encoding CHARSET = Encoding.UTF8; // UTF-8

    // For creating data buffers
    public Page(int blockSize)
    {
        _stream = new MemoryStream(new byte[blockSize]);
        _writer = new BinaryWriter(_stream, CHARSET);
        _reader = new BinaryReader(_stream, CHARSET);
    }

    // For creating log pages
    public Page(byte[] bytes)
    {
        _stream = new MemoryStream(bytes);
        _writer = new BinaryWriter(_stream, CHARSET); // CHARSETを指定
        _reader = new BinaryReader(_stream, CHARSET); // CHARSETを指定
    }

    public int GetInt(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        return _reader.ReadInt32();
    }

    public void SetInt(int offset, int n)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _writer.Write(n);
    }

    public byte[] GetBytes(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        int length = _reader.ReadInt32();
        return _reader.ReadBytes(length); //ReadBytesを使うことでEndOfStreamExceptionをハンドル
    }

    public void SetBytes(int offset, byte[] bytes)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _writer.Write(bytes.Length); // length (int) を書き込む
        _writer.Write(bytes);
    }

    public string GetString(int offset)
    {
        return CHARSET.GetString(GetBytes(offset));
    }

    public void SetString(int offset, string s)
    {
        byte[] bytes = CHARSET.GetBytes(s);
        SetBytes(offset, bytes);
    }

    //文字列の長さ(int)+文字列自体のバイト配列の長さを返す。
    public static int MaxLength(int strlen)
    {
        //文字列のバイト数(int)+文字列の長さ(int)=必要なバイト数
        return Bytes.Integer + Bytes.Integer + CHARSET.GetMaxByteCount(strlen);
    }

    internal void SetContents(Span<byte> buffer)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.Write(buffer);
    }

    internal byte[] Contents()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        return _stream.ToArray();
    }

    // 新しいAppendLogRecordメソッド
    internal int AppendLogRecord(byte[] logrec)
    {
        _stream.Seek(0, SeekOrigin.End); //末尾に移動
        int recsize = logrec.Length;
        int bytesneeded = recsize + Bytes.Integer; // レコードサイズ +　自身のレコードのサイズ(int)
        if (_stream.Position + bytesneeded > _stream.Length)
        {
            return -1; //エラー
        }
        int currentPos = (int)_stream.Position;
        _writer.Write(recsize); //自分自身のサイズ
        _writer.Write(logrec); //レコードを書き込む

        return currentPos;
    }

    internal int GetNextRecordOffset(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        return _reader.ReadInt32();
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
                _writer.Dispose(); // BinaryWriterをDispose
                _reader.Dispose(); // BinaryReaderをDispose
                _stream.Dispose(); // MemoryStreamをDispose (Close()ではなくDispose()を推奨)
            }

            // アンマネージドリソースがあれば解放 (Pageクラスにはない)

            IsDisposed = true;
        }
    }

    ~Page()
    {
        Dispose(false);
    }
}
