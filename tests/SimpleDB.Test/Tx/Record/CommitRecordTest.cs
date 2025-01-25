using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Record;

public class CommitRecordTest
{
    [Fact]
    public void ToString_トランザクション番号と共に値が取得できる()
    {
        var page = new Page(1024);
        var record = new CommitRecord(page);

        var actual = record.ToString();

        Assert.Equal("<COMMIT 0>", actual);
    }

    [Fact]
    public void WriteToLog_Commitに相当する値がLogManagerに渡される()
    {
        var lm = A.Fake<ILogManager>();

        var record = CommitRecord.WriteToLog(lm, 1);

        A.CallTo(() => lm.Append(A<byte[]>.Ignored)).MustHaveHappened();
    }
}
