using System.Collections;
using Common;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

internal class LogIterator : IEnumerable<byte[]>, IDisposable
{
    private readonly IFileManager _fm;

    private BlockId _blockId;

    private Page _page;

    private int _currentPos;

    private int _boundary;

    private bool _disposed = false;

    public LogIterator(IFileManager fm, BlockId blockId)
    {
        _fm = fm;
        _blockId = blockId;
        var bytes = new byte[fm.BlockSize];
        _page = new Page(bytes);
        MoveToBlock(_blockId);
    }

    public IEnumerator<byte[]> GetEnumerator()
    {
        while (HasNext())
        {
            if (_currentPos == _fm.BlockSize)
            {
                _blockId = new BlockId(_blockId.FileName, _blockId.Number - 1);
                MoveToBlock(_blockId);
            }

            byte[] rec = _page.GetBytes(_currentPos);
            _currentPos += sizeof(int) + rec.Length;
            yield return rec;

            if (_disposed) // disposedされた後にyield returnしないようにする
            {
                yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private bool HasNext()
    {
        return _currentPos < _fm.BlockSize || _blockId.Number > 0;
    }

    // IDisposableの実装
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _page?.Dispose();
        }

        _disposed = true;
    }

    ~LogIterator() // デストラクタ（ファイナライザ）
    {
        Dispose(false);
    }

    private void MoveToBlock(BlockId blk)
    {
        // 既にPageオブジェクトが存在する場合は破棄する
        _page?.Dispose();
        _page = new Page(new byte[_fm.BlockSize]); // 新しいPageオブジェクトを作成
        _fm.Read(blk, _page);
        _boundary = _page.GetInt(0);
        _currentPos = _boundary;
    }
}
