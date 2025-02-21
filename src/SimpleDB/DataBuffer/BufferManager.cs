using System.Diagnostics;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.DataBuffer;

internal sealed class BufferManager : IBufferManager
{
    private readonly List<Buffer> _bufferPool;

    private int _availableBufferCount;

    private static readonly int MAX_TIME = 1000; // 1 seconds

    // Singleton instance using Lazy<T>
    private static Lazy<BufferManager>? s_lazyInstance;

    private BufferManager(IFileManager fileManager, ILogManager logManager, int bufferCount)
    {
        _bufferPool = [];
        _availableBufferCount = bufferCount;
        for (int i = 0; i < bufferCount; i++)
        {
            _bufferPool.Add(new Buffer(fileManager, logManager));
        }
    }

    private static readonly object InitLock = new(); // 初期化時のロック用

    public static BufferManager GetInstance(
        IFileManager fileManager,
        ILogManager logManager,
        int bufferCount
    )
    {
        lock (InitLock) // 排他制御
        {
            if (s_lazyInstance == null)
            {
                // Lazy<T> の初期化 (コンストラクタ呼び出し)
                s_lazyInstance = new Lazy<BufferManager>(
                    () => new BufferManager(fileManager, logManager, bufferCount)
                );
            }
            else // もし初期化済なら、渡された引数は無視
            {
                // 必要ならここで警告ログなどを出す
                Console.WriteLine("Warning: BufferManager is already initialized.");
            }

            return s_lazyInstance.Value; // 必ず Value が存在する
        }
    }

    private readonly object _lockObject = new();

    public void FlushAll(int txNumber)
    {
        lock (_lockObject)
        {
            foreach (var buffer in _bufferPool)
            {
                if (buffer.ModifyingTx == txNumber)
                {
                    buffer.Flush();
                }
            }
        }
    }

    public void Unpin(Buffer buffer)
    {
        lock (_lockObject)
        {
            buffer.Unpin();
            if (!buffer.IsPinned)
            {
                _availableBufferCount++;
                // NOTE: ここのコードは本当にこれでよいのか？
                Monitor.PulseAll(_lockObject);
            }
        }
    }

    public Buffer Pin(BlockId blockId)
    {
        lock (_lockObject)
        {
            var stopWatch = new Stopwatch();
            Buffer? buffer;
            try
            {
                stopWatch.Start();
                buffer = TryToPin(blockId);
                while (buffer is null && !WaitingTooLong(stopWatch))
                {
                    Monitor.Wait(_lockObject, MAX_TIME);
                    buffer = TryToPin(blockId);
                }
            }
            catch (ThreadInterruptedException e)
            {
                throw new BufferAbortException(e.Message);
            }

            return buffer ?? throw new BufferAbortException("No buffer is available.");
        }
    }

    private static bool WaitingTooLong(Stopwatch stopWatch)
    {
        var elapsed = stopWatch.ElapsedMilliseconds;
        return elapsed > MAX_TIME;
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

    private Buffer? FindExistingBuffer(BlockId blockId) =>
        _bufferPool.FirstOrDefault((buffer) => blockId.Equals(buffer?.Block));

    private Buffer? ChooseUnpinnedBuffer() =>
        _bufferPool.FirstOrDefault((buffer) => !buffer.IsPinned);

    public int Available()
    {
        lock (_lockObject)
        {
            return _availableBufferCount;
        }
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            foreach (var buffer in _bufferPool)
            {
                buffer.Dispose();
            }
        }
    }

#if DEBUG
    // Internal method for resetting the instance ONLY during testing.
    internal static void ResetInstanceForTesting()
    {
        lock (InitLock)
        {
            s_lazyInstance = null;
        }
    }
#endif
}
