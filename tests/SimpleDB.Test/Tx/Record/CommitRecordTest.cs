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

    [Fact(Skip = "TODO")]
    public void WriteToLog()
    {
        var lm = A.Fake<ILogManager>();

        var record = CommitRecord.WriteToLog(lm, 0);

        A.CallTo(() => lm.Append(A<byte[]>.That.Matches(x => x == new byte[10])))
            .MustHaveHappened();
    }
}
