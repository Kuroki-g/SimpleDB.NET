using SimpleDB.Storage;
using SimpleDB.Tx.Concurrency;

namespace SimpleDB.Feat.Test.Tx.Concurrency;

public class ConcurrencyManagerTest : IntegrationTestBase
{
    [Fact]
    public void SharedLock()
    {
        var blockId = new BlockId("file-name", 1);

        var cm = new ConcurrencyManager();
        cm.SharedLock(blockId);

        var actual = cm.GetLockType(blockId);

        Assert.Equal(LockType.SHARED, actual);
    }

    [Fact]
    public void ExclusiveLock()
    {
        var blockId = new BlockId("file-name", 1);

        var cm = new ConcurrencyManager();

        cm.ExclusiveLock(blockId);
        var actual = cm.GetLockType(blockId);

        Assert.Equal(LockType.EXCLUSIVE, actual);
    }
}
