using System.Runtime.CompilerServices;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx.Concurrency;
using SimpleDB.Tx.Recovery;

namespace SimpleDB.Tx;

public class Transaction : ITransaction
{
    private static int s_nextNum = 0;

    private static readonly int END_OF_FILE = -1;

    internal readonly IFileManager Fm;

    internal readonly ILogManager Lm;

    internal readonly IBufferManager Bm;

    internal readonly IRecoveryManager Rm;

    internal readonly IConcurrencyManager Cm;

    private readonly BufferBoard _bufferBoard;

    internal int TxNumber { get; }

    public Transaction(IFileManager fm, ILogManager lm, IBufferManager bm)
    {
        Fm = fm;
        Bm = bm;
        TxNumber = NextTxNumber();
        Lm = lm;
        Rm = new RecoveryManager(this, TxNumber, Lm, Bm);
        Cm = new ConcurrencyManager();
        _bufferBoard = new BufferBoard(Bm);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static int NextTxNumber()
    {
        s_nextNum++;
        return s_nextNum;
    }

    public BlockId Append(string fileName)
    {
        var dummyBlock = new BlockId(fileName, END_OF_FILE);
        Cm.ExclusiveLock(dummyBlock);
        return Fm.Append(fileName);
    }

    public int AvailableBuffers() => Bm.Available();

    public int BlockSize() => Fm.BlockSize;

    public void Commit()
    {
        Rm.Commit();
        Cm.Release();
        _bufferBoard.UnpinAll();
        // add logger if requied.
    }

    public void Rollback()
    {
        Rm.Rollback();
        Cm.Release();
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
        Cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        return buffer.Contents.GetInt(offset);
    }

    public string GetString(BlockId blockId, int offset)
    {
        Cm.ExclusiveLock(blockId);
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
        Bm.FlushAll(TxNumber);
        Rm.Recover();
    }

    public void SetInt(BlockId blockId, int offset, int value, bool okToLog)
    {
        Cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        var lsn = -1;
        if (okToLog)
        {
            lsn = Rm.SetInt(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.SetInt(offset, value);
        buffer.SetModified(TxNumber, lsn);
    }

    public void SetString(BlockId blockId, int offset, string value, bool okToLog)
    {
        Cm.ExclusiveLock(blockId);
        var buffer =
            _bufferBoard.GetBuffer(blockId)
            ?? throw new BufferAbortException("buffer was not found.");
        var lsn = -1;
        if (okToLog)
        {
            lsn = Rm.SetString(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.SetString(offset, value);
        buffer.SetModified(TxNumber, lsn);
    }

    public int Size(string fileName)
    {
        var dummyBlock = new BlockId(fileName, END_OF_FILE);
        Cm.SharedLock(dummyBlock);

        return Fm.Length(fileName);
    }

    /// <summary>
    /// 指定のブロックをunpinする。
    /// </summary>
    /// <param name="blockId"></param>
    public void Unpin(BlockId blockId) => _bufferBoard.Unpin(blockId);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Fm.Dispose();
    }
}
