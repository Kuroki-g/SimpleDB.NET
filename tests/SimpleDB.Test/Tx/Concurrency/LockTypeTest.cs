using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Tx.Concurrency;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Concurrency;

public class LockTypeTest
{
    [Fact]
    public void LockType_default()
    {
        var actual = default(LockType);

        Assert.Equal(LockType.NONE, actual);
    }
}
