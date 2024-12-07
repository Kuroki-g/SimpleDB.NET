using System.Diagnostics;
using System.Runtime.CompilerServices;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.DataBuffer;

public sealed class BufferManager
{
    private readonly List<Buffer> _bufferPool;

    private int _availableBufferCount;

    private static int _MAX_TIME = 1000; // 1 seconds

    public BufferManager(IFileManager fileManager, ILogManager logManager, int bufferCount)
    {
        _bufferPool = [];
        _availableBufferCount = bufferCount;
        for (int i = 0; i < bufferCount; i++)
        {
            _bufferPool.Add(new Buffer(fileManager, logManager));
        }
    }

    public int Available
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _availableBufferCount;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void FlushAll(int txNumber)
    {
        _bufferPool.ForEach(
            (buffer) =>
            {
                if (buffer.ModifiyingTx == txNumber)
                {
                    buffer.Flush();
                }
            }
        );
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unpin(Buffer buffer)
    {
        buffer.Unpin();
        if (!buffer.IsPinned)
        {
            _availableBufferCount++;
            // NOTE: ここのコードは本当にこれでよいのか？
            Monitor.PulseAll(this);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Buffer Pin(BlockId blockId)
    {
        var stopWatch = new Stopwatch();
        Buffer? buffer;
        try
        {
            stopWatch.Start();
            buffer = TryToPin(blockId);
            while (buffer is null && !WaitingTooLong(stopWatch))
            {
                Monitor.Wait(this, _MAX_TIME);
                buffer = TryToPin(blockId);
            }
        }
        catch (ThreadInterruptedException e)
        {
            throw new BufferAbortException(e.Message);
        }

        return buffer ?? throw new BufferAbortException("No buffer is available.");
    }

    private static bool WaitingTooLong(Stopwatch stopWatch)
    {
        var elapsed = stopWatch.ElapsedMilliseconds;
        return elapsed > _MAX_TIME;
    }

    private Buffer? TryToPin(BlockId blockId)
    {
        var buffer = FindExistingBuffer(blockId);
        if (buffer is null)
        {
            buffer = ChooseUnpinnedBuffer();
            if (buffer is null)
                return null;
            buffer.AssignToBlock(blockId);
        }
        if (!buffer.IsPinned)
            _availableBufferCount--;
        buffer.Pin();

        return buffer;
    }

    private Buffer? FindExistingBuffer(BlockId blockId)
    {
        return _bufferPool.FirstOrDefault((buffer) => blockId.Equals(buffer?.Block));
    }

    private Buffer? ChooseUnpinnedBuffer()
    {
        return _bufferPool.FirstOrDefault((buffer) => !buffer.IsPinned);
    }
}
