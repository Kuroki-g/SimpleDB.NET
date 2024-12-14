using Microsoft.VisualBasic;
using SimpleDB.DataBuffer;
using SimpleDB.Feat.Test.Tx;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Structure;
using Transaction = SimpleDB.Tx.Transaction;

namespace SimpleDB.Feat.Test.Structure;

public class RecordTest : IntegrationTestBase
{
    [Fact]
    public void RecordTest1()
    {
        var fm = new FileManager(_dir, 400);
        var lm = new LogManager(fm, "log-file");
        var bm = new BufferManager(fm, lm, 8);

        var tx = new Transaction(fm, lm, bm);

        var schema = new Schema();
        schema.AddIntField("A");
        schema.AddStringField("B", 9);
        var layout = new Layout(schema);

        var blockId = tx.Append("test-file");
        tx.Pin(blockId);

        var recordPage = new RecordPage(tx, blockId, layout);
        recordPage.Format();

        var slot = recordPage.InsertAfter(-1);
        while (slot >= 0)
        {
            var a = recordPage.GetInt(slot, "A");
            var b = recordPage.GetString(slot, "B");
            slot = recordPage.NextAfter(slot);

            Assert.Equal(default, a);
            Assert.Equal(string.Empty, b);
        }
        tx.Unpin(blockId); // TODO: feature testにこれは必要か？
        tx.Commit(); // TODO: feature testにこれは必要か？
    }
}
