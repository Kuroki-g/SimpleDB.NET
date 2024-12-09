using SimpleDB.Storage;

namespace SimpleDB.Tx.Concurrency;

public class ConcurrencyManager : IConcurrencyManager
{
    private readonly LockTable _lockTable = new();

    private readonly Dictionary<BlockId, LockType> _locks = [];

    public void SharedLock(BlockId blockId)
    {
        if (_locks.GetValueOrDefault(blockId) == LockType.NONE)
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

    private bool HasExclusiveLock(BlockId blockId) =>
        _locks.GetValueOrDefault(blockId).Equals(LockType.EXCLUSIVE);
}
