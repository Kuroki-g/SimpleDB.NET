using SimpleDB.Storage;
using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Tx.Concurrency;

public interface IConcurrencyManager
{
    public void ExclusiveLock(BlockId blockId);

    public void SharedLock(BlockId blockId);

    public void Release();
}
