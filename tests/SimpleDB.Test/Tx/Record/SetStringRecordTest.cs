using Common;
using SimpleDB.Storage;
using SimpleDB.Tx.Recovery.LogRecord;

namespace SimpleDB.Test.Tx.Record;

public class SetStringRecordTest
{
    [Fact]
    public void ToString_トランザクション番号と共に値が取得できる()
    {
        var tPos = Bytes.Integer;
        var txNumber = 0x01;
        var page = new Page(0xFF * 10);
        page.SetInt(tPos, txNumber);

        var fPos = tPos + Bytes.Integer;
        page.SetString(fPos, "file-name");

        // Act
        var record = new SetStringRecord(page);

        var actual = record.ToString();

        Assert.Equal("<SETSTRING 1 [file file-name, block 0] 0 >", actual);
    }
}
