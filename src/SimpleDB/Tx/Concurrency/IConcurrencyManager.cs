using SimpleDB.Storage;

namespace SimpleDB.Tx.Concurrency;

public interface IConcurrencyManager
{
    /// <summary>
    /// 指定のブロックの排他ロックを獲得する。
    /// </summary>
    /// <param name="blockId"></param>
    public void ExclusiveLock(BlockId blockId);

    /// <summary>
    /// 指定のブロックの共有ロックを獲得する。
    /// </summary>
    /// <param name="blockId"></param>
    public void SharedLock(BlockId blockId);

    public void Release();
}
