using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Record;

public class RollbackRecordTest
{
    [Fact]
    public void ToString_トランザクション番号と共に値が取得できる()
    {
        var page = new Page(1024);
        var record = new RollbackRecord(page);

        var actual = record.ToString();

        Assert.Equal("<ROLLBACK 0>", actual);
    }

    [Fact(Skip = "TODO")]
    public void WriteToLog()
    {
        var lm = A.Fake<ILogManager>();

        var record = RollbackRecord.WriteToLog(lm, 1);

        A.CallTo(() => lm.Append(A<byte[]>.That.Matches(x => x == new byte[10])))
            .MustHaveHappened();
    }
}
