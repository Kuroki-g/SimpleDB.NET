using System.Runtime.CompilerServices;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx.Concurrency;
using SimpleDB.Tx.Recovery;

namespace SimpleDB.Tx;

public class Transaction : ITransaction, IDisposable
{
    private static int _nextNum = 0;

    private static readonly int _END_OF_FILE = -1;

    private readonly IFileManager _fm;

    private readonly ILogManager _lm;

    private readonly IBufferManager _bm;

    private readonly IRecoveryManager _rm;

    private readonly IConcurrencyManager _cm;

    private readonly BufferBoard _bufferBoard;

    private int _txNumber;

    public Transaction(IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        _fm = fm;
        _bm = bm;
        _txNumber = NextTxNumber();
        _lm = lm;
        _rm = new RecoveryManager(this, _txNumber, _lm, _bm);
        _cm = new ConcurrencyManager();
        _bufferBoard = new BufferBoard(_bm);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static int NextTxNumber()
    {
        _nextNum++;
        return _nextNum;
    }

    public BlockId Append(string fileName)
    {
        var dummyBlock = new BlockId(fileName, _END_OF_FILE);
        _cm.ExclusiveLock(dummyBlock);
        return _fm.Append(fileName);
    }

    public int AvailableBuffers() => _bm.Available();

    public int BlockSize() => _fm.BlockSize;

    public void Commit()
    {
        _rm.Commit();
        _cm.Release();
        _bufferBoard.UnpinAll();
        // add logger if requied.
    }

    public void Rollback()
    {
        _rm.Rollback();
        _cm.Release();
        _bufferBoard.UnpinAll();
        // add logger
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int GetInt(BlockId blockId, int offset)
    {
        _cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        return buffer.Contents.GetInt(offset);
    }

    public string GetString(BlockId blockId, int offset)
    {
        _cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");

        return buffer.Contents.GetString(offset);
    }

    /// <summary>
    /// 指定のブロックをPinする。
    /// </summary>
    /// <param name="blockId"></param>
    public void Pin(BlockId blockId) => _bufferBoard.Pin(blockId);

    public void Recover()
    {
        _bm.FlushAll(_txNumber);
        _rm.Recover();
    }

    public void SetInt(BlockId blockId, int offset, int value, bool okToLog)
    {
        _cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        var lsn = -1;
        if (okToLog)
        {
            lsn = _rm.SetInt(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.SetInt(offset, value);
        buffer.SetModified(_txNumber, lsn);
    }

    public void SetString(BlockId blockId, int offset, string value, bool okToLog)
    {
        _cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        var lsn = -1;
        if (okToLog)
        {
            lsn = _rm.SetString(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.SetString(offset, value);
        buffer.SetModified(_txNumber, lsn);
    }

    public int Size(string fileName)
    {
        var dummyBlock = new BlockId(fileName, _END_OF_FILE);
        _cm.SharedLock(dummyBlock);

        return _fm.Length(fileName);
    }

    /// <summary>
    /// 指定のブロックをunpinする。
    /// </summary>
    /// <param name="blockId"></param>
    public void Unpin(BlockId blockId) => _bufferBoard.Unpin(blockId);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _fm.Dispose();
    }
}
