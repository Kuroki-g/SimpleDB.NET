using System.Collections;
using Common;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

internal class LogEnumerator : IEnumerator<byte[]>, IDisposable
{
    private readonly IFileManager _fm;

    private BlockId _blockId;

    private readonly Page _page;

    private int _currentPos;

    private bool _disposed = false;

    public LogEnumerator(IFileManager fm, BlockId blockId)
    {
        _fm = fm;
        _blockId = blockId;
        var bytes = new byte[fm.BlockSize];
        _page = new Page(bytes);
        MoveToBlock(_blockId);
    }

    public byte[] Current => _page.GetBytes(_currentPos);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _page.Dispose();
        }

        _disposed = true;
    }

    public bool MoveNext()
    {
        // まずJavaのコードのhasNextを行う。falseならそのままreturnする。
        var hasNext = _currentPos < _fm.BlockSize || _blockId.Number > 0;
        if (!hasNext)
            return false;
        // next相当のコードを呼び出す。
        if (_currentPos == _fm.BlockSize)
        {
            _blockId = new BlockId(_blockId.FileName, _blockId.Number - 1);
            MoveToBlock(_blockId);
        }

        // TODO: _currentPos += sizeof(int) + Current.Length;の方が良いかもしれない。
        _currentPos += Bytes.Integer + Current.Length; //変更前
        return true;
    }

    public void Reset()
    {
        throw new InvalidOperationException("Cannot revert log block");
    }

    /// <summary>
    /// Moves to the specified log block and positions it at the first record in that block (i.e., the most recent one).
    /// </summary>
    /// <param name="blockId"></param>
    private void MoveToBlock(BlockId blockId)
    {
        _fm.Read(blockId, _page);
        var boundary = _page.GetInt(0);
        _currentPos = boundary;
    }
}
