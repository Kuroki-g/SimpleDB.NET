using SimpleDB.Storage;
using SimpleDB.Tx.Concurrency;

namespace SimpleDB.Feat.Test.Tx;

public class LockTableTest
{
    [Fact]
    public void SharedLock_zero_if_no_lock()
    {
        var table = new LockTable();

        Assert.Equal(0, table.LockCount);
    }

    [Fact]
    public void SharedLock_shared_lock_increment_lock_count()
    {
        var table = new LockTable();

        var blockId = new BlockId("file-name", 1);

        table.SharedLock(blockId);

        Assert.Equal(1, table.LockCount);
    }

    [Fact]
    public void SharedLock_shared_lock_two_times()
    {
        var table = new LockTable();

        var blockId = new BlockId("file-name", 1);
        var blockId2 = new BlockId("file-name2", 2);

        table.SharedLock(blockId);
        table.SharedLock(blockId2);

        Assert.Equal(2, table.LockCount);
    }

    [Fact]
    public void SharedLock_cannot_get_exclusive_lock()
    {
        var table = new LockTable();

        var blockId = new BlockId("file-name", 1);
        table.ExclusiveLock(blockId); // lock first

        var exception = Record.Exception(() => table.SharedLock(blockId));

        Assert.IsType<LockAbortException>(exception);
    }

    [Fact]
    public void ExclusiveLock_exclusive_lock_increment_lock_count()
    {
        var table = new LockTable();

        var blockId = new BlockId("file-name", 1);

        table.ExclusiveLock(blockId);

        Assert.Equal(1, table.LockCount);
    }

    [Fact]
    public void ExclusiveLock_lock_two_times_not_increment_lock_count()
    {
        var table = new LockTable();

        var blockId = new BlockId("file-name", 1);
        table.ExclusiveLock(blockId);
        table.ExclusiveLock(blockId);

        // 同じblockに対してlockしても占有ロックの場合には増えない。
        Assert.Equal(1, table.LockCount);
    }
}
