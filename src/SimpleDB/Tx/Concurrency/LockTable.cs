using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SimpleDB.Storage;

namespace SimpleDB.Tx.Concurrency;

/// <summary>
/// TODO: 静的にインスタンスを保持する。スレッドセーフな実装をしなくてはいけない。
/// </summary>
public sealed class LockTable : ILockTable
{
    /// <summary>
    /// Execution max time (millisecond)
    /// </summary>
    private static readonly int _MAX_TIME = 1000;

    /// <summary>
    ///
    /// </summary>
    /// <remarks>KeyValuePairのほうがパフォーマンスが良くなるかも</remarks>
    private readonly Dictionary<BlockId, int> _locks = [];

    internal int LockCount => _locks.Count;

    internal ReadOnlyDictionary<BlockId, int> Locks => new(_locks);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SharedLock(BlockId blockId)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (HasExclusiveLock(blockId) && !WaitingTooLong(timestamp))
            {
                Monitor.Wait(this, _MAX_TIME);
            }
            if (HasExclusiveLock(blockId))
            {
                throw new LockAbortException();
            }
            var value = GetLockValue(blockId);
            _locks[blockId] = value + 1;
        }
        catch (Exception e)
        {
            throw new LockAbortException(e.Message);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void ExclusiveLock(BlockId blockId)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (HasOtherSharedLocks(blockId) && !WaitingTooLong(timestamp))
            {
                Monitor.Wait(this, _MAX_TIME);
            }
            if (HasOtherSharedLocks(blockId))
            {
                throw new LockAbortException();
            }
            _locks[blockId] = -1;
        }
        catch (Exception e)
        {
            throw new LockAbortException(e.Message);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void UnLock(BlockId blockId)
    {
        var value = GetLockValue(blockId);
        if (value > 1)
        {
            _locks[blockId] = value - 1;
        }
        else
        {
            _locks.Remove(blockId);
            Monitor.PulseAll(this);
        }
    }

    private bool HasExclusiveLock(BlockId blockId) => GetLockValue(blockId) < 0;

    private bool HasOtherSharedLocks(BlockId blockId) => GetLockValue(blockId) > 1;

    private static bool WaitingTooLong(long startTime) =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > _MAX_TIME;

    /// <summary>
    ///
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns>defaultは0</returns>
    private int GetLockValue(BlockId blockId) => _locks.GetValueOrDefault(blockId);
}
