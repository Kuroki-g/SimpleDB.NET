using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
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

    [Fact(Skip = "TODO")]
    public void WriteToLog()
    {
        var lm = A.Fake<ILogManager>();

        var record = CheckpointRecord.WriteToLog(lm);

        A.CallTo(() => lm.Append(A<byte[]>.That.Matches(x => x == new byte[10])))
            .MustHaveHappened();
    }
}
