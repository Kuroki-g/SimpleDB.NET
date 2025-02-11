// LogEnumerator.cs
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
        _currentPos = PreviousRecordPosition();

        if (_currentPos == 0) //最後の位置
        {
            if (_blockId.Number == 0)
            {
                throw new InvalidOperationException("No next log record."); // 変更: 例外をスロー
            }
            else
            {
                _blockId = new BlockId(_blockId.FileName, _blockId.Number - 1); //前のブロックへ
                MoveToBlock(_blockId);
                if (_currentPos == 0)
                {
                    throw new InvalidOperationException("No next log record."); // 変更: 例外をスロー
                }
            }
        }
        return true;
    }

    private int PreviousRecordPosition()
    {
        if (_currentPos == 0)
            return 0; //ここは残す
        int currentRecordSize = _page.GetInt(_currentPos); // 現在のレコードのサイズを取得
        int previousPos = _currentPos - (currentRecordSize + Bytes.Integer); // 前のレコードの位置を計算
        return previousPos >= 0 ? previousPos : 0; //境界値チェック
    }

    public void Reset()
    {
        throw new InvalidOperationException("Cannot revert log block");
    }

    private void MoveToBlock(BlockId blockId)
    {
        _fm.Read(blockId, _page);
        _currentPos = _page.GetInt(0); //最後のレコードを指すように
    }
}
