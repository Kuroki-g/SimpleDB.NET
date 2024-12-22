using SimpleDB.Storage;

namespace SimpleDB.Tx.Concurrency;

public class ConcurrencyManager : IConcurrencyManager
{
    private readonly LockTable _lockTable = new();

    private readonly Dictionary<BlockId, LockType> _locks = [];

    private readonly int LOCK_MAX_COUNT = short.MaxValue;

    public void SharedLock(BlockId blockId)
    {
        if (_locks.Count > LOCK_MAX_COUNT)
        {
            throw new LockAbortException("too much lock was used. please check your operation.");
        }
        if (GetLockType(blockId) == LockType.NONE)
        {
            _lockTable.SharedLock(blockId);
            _locks[blockId] = LockType.SHARED;
        }
    }

    public void ExclusiveLock(BlockId blockId)
    {
        if (HasExclusiveLock(blockId))
            return;

        SharedLock(blockId);
        _lockTable.ExclusiveLock(blockId);
        _locks[blockId] = LockType.EXCLUSIVE;
    }

    public void Release()
    {
        foreach (var blockId in _locks.Keys)
        {
            _lockTable.UnLock(blockId);
        }
        _locks.Clear();
    }

    /// <summary>
    /// 指定のブロックのロックの状態を獲得する。
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns><see cref="LockType" /></returns>
    internal LockType GetLockType(BlockId blockId) => _locks.GetValueOrDefault(blockId);

    private bool HasExclusiveLock(BlockId blockId) =>
        GetLockType(blockId).Equals(LockType.EXCLUSIVE);
}
