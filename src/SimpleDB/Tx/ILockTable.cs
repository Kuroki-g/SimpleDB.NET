using SimpleDB.Storage;

namespace SimpleDB.Tx;

public interface ILockTable
{
    public void SharedLock(BlockId blockId);

    public void ExclusiveLock(BlockId blockId);

    public void UnLock(BlockId blockId);
}
