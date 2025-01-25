using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Record;

public class CheckpointRecordTest
{
    [Fact]
    public void ToString_トランザクション番号と共に値が取得できる()
    {
        var record = new CheckpointRecord();

        var actual = record.ToString();

        Assert.Equal("<CHECKPOINT>", actual);
    }

    [Fact]
    public void WriteToLog_Checkpointに相当する値がLogManagerに渡される()
    {
        var lm = A.Fake<ILogManager>();

        var record = CheckpointRecord.WriteToLog(lm);

        A.CallTo(() => lm.Append(A<byte[]>.Ignored)).MustHaveHappened();
    }
}
