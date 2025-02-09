using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx;
using SimpleDB.Tx.Recovery;

namespace SimpleDB.Feat.Test.Tx;

public class RecoveryManagerTest : IntegrationTestBase
{
    [Fact]
    public void Recovery_ShouldRecoverUncommittedTransactions()
    {
        // Arrange
        using var tx = CreateTransaction();
        var lm = A.Fake<ILogManager>();
        var bm = A.Fake<IBufferManager>();
        var recoveryManager = new RecoveryManager(tx, 1, lm, bm);

        // Act
        recoveryManager.Recover();

        // Assert
        A.CallTo(() => lm.Flush(A<int>.Ignored)).MustHaveHappened();
        A.CallTo(() => bm.FlushAll(1)).MustHaveHappened();
    }

    [Fact]
    public void Commit_ShouldWriteCommitRecordAndFlush()
    {
        // Arrange
        var tx = CreateTransaction();
        var lm = A.Fake<ILogManager>();
        var bm = A.Fake<IBufferManager>();
        var recoveryManager = new RecoveryManager(tx, tx.TxNumber, lm, bm);

        // Act
        recoveryManager.Commit();

        // Assert
        A.CallTo(() => lm.Flush(A<int>.Ignored)).MustHaveHappened();
        A.CallTo(() => bm.FlushAll(tx.TxNumber)).MustHaveHappened();
    }

    [Fact]
    public void Rollback_ShouldWriteRollbackRecordAndFlush()
    {
        // Arrange
        var tx = CreateTransaction();
        var lm = A.Fake<ILogManager>();
        var bm = A.Fake<IBufferManager>();
        var recoveryManager = new RecoveryManager(tx, tx.TxNumber, lm, bm);

        // Act
        recoveryManager.Rollback();

        // Assert
        A.CallTo(() => lm.Flush(A<int>.Ignored)).MustHaveHappened();
        A.CallTo(() => bm.FlushAll(tx.TxNumber)).MustHaveHappened();
    }
}
