using SimpleDB.Storage;
using SimpleDB.Tx.Recovery;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Recovery;

public class ILogRecordTest
{
    [Fact]
    public void Create_ShouldReturnCheckpointRecord_WhenStatusIsCheckpoint()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.CHECKPOINT);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<CheckpointRecord>(record);
    }

    [Fact]
    public void Create_ShouldReturnStartRecord_WhenStatusIsStart()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.START);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<StartRecord>(record);
    }

    [Fact]
    public void Create_ShouldReturnCommitRecord_WhenStatusIsCommit()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.COMMIT);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<CommitRecord>(record);
    }

    [Fact]
    public void Create_ShouldReturnRollbackRecord_WhenStatusIsRollback()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.ROLLBACK);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<RollbackRecord>(record);
    }

    [Fact(Skip = "Not implemented")]
    public void Create_ShouldReturnSetIntRecord_WhenStatusIsSetInt()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.SETINT);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<SetIntRecord>(record);
    }

    [Fact(Skip = "Not implemented")]
    public void Create_ShouldReturnSetStringRecord_WhenStatusIsSetString()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, (int)TransactionStatus.SETSTRING);

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.IsType<SetStringRecord>(record);
    }

    [Fact]
    public void Create_ShouldReturnNull_WhenStatusIsUnknown()
    {
        // Arrange
        var bytes = new byte[100];
        var page = new Page(bytes);
        page.SetInt(0, -1); // Unknown status

        // Act
        var record = ILogRecord.Create(bytes);

        // Assert
        Assert.Null(record);
    }
}
