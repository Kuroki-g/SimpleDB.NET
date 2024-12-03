using System.Collections;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

internal sealed class LogIterator : IEnumerator<byte[]>
{
    private readonly IFileManager _fm;

    private BlockId _blockId;

    private readonly Page _page;

    private int _currentPos;

    public LogIterator(IFileManager fileManager, BlockId blockId)
    {
        _fm = fileManager;
        _blockId = blockId;
        byte[] bytes = new byte[_blockId.Number];
        _page = new Page(bytes);
        Current = []; // ほんとうにこれでよいのか？
    }


    public byte[] Current { get; private set; }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _page.Dispose();
    }

    public bool MoveNext()
    {
        // まずJavaのコードのhasNextを行う。falseならそのままreturnする。
        var hasNext = _currentPos < _fm.BlockSize || _blockId.Number > 0;
        if (!hasNext) return false;
        // next相当のコードを呼び出す。
        if (_currentPos == _fm.BlockSize)
        {
            _blockId = new BlockId(_blockId.FileName, _blockId.Number - 1);
            MoveToBlock(_blockId);
        }
        Current = _page.GetBytes(_currentPos);
        // _currentPos += Integer.BYTES + Current.Length;のはず。Integer.BYTESは何なのだろう
        _currentPos += Current.Length;
        return true;
    }

    public void Reset()
    {
        throw new InvalidOperationException("Cannot revert log block");
    }

    /// <summary>
    /// Moves to the specified log block and positions it at the first record in that block (i.e., the most recent one).
    /// </summary>
    /// <param name="blk"></param>
    private void MoveToBlock(BlockId blk)
    {
        _fm.Read(_blockId, _page);
        var boundary = _page.GetInt(0);
        _currentPos = boundary;
    }
}
