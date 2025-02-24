using SimpleDB.Feat.Test.Tx;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Structure;

public class RecordTest : IntegrationTestBase
{
    [Fact]
    public void Transaction_DBへのインサートが実行できる()
    {
        using var tx = CreateTransaction();

        var sch = new Schema();
        sch.AddIntField("A");
        sch.AddStringField("B", 9);
        var layout = new Layout(sch);
        var actualOffsets = new Dictionary<string, int>();
        foreach (var fieldName in layout.Schema.Fields)
        {
            var offset = layout.Offset(fieldName);
            actualOffsets[fieldName] = offset;
        }
        Assert.Equal(4, actualOffsets["A"]);
        Assert.Equal(8, actualOffsets["B"]);

        // 新しいファイルにブロックを作成し、レコードを挿入する
        var blk = tx.Append("testfile2");
        tx.Pin(blk);
        var rp = new RecordPage(tx, blk, layout);
        rp.Format();

        // Filling the page with random records.
        int slot = rp.InsertAfter(-1);
        while (slot >= 0)
        {
            int n = (int)Math.Round((double)_random.Next() * 50);
            rp.SetInt(slot, "A", n);
            rp.SetString(slot, "B", "rec" + n);
            Console.WriteLine("inserting into slot " + slot + ": {" + n + ", " + "rec" + n + "}");
            slot = rp.InsertAfter(slot);
        }

        Console.WriteLine("Deleting these records, whose A-values are less than 25.");
        int count = 0;
        slot = rp.NextAfter(-1);
        while (slot >= 0)
        {
            int a = rp.GetInt(slot, "A");
            var b = rp.GetString(slot, "B");
            if (a < 25)
            {
                count++;
                Console.WriteLine("slot " + slot + ": {" + a + ", " + b + "}");
                rp.Delete(slot);
            }
            slot = rp.NextAfter(slot);
        }
        Console.WriteLine(count + " values under 25 were deleted.\n");

        Console.WriteLine("Here are the remaining records.");
        slot = rp.NextAfter(-1);
        while (slot >= 0)
        {
            var a = rp.GetInt(slot, "A");
            var b = rp.GetString(slot, "B");
            Console.WriteLine("slot " + slot + ": {" + a + ", " + b + "}");
            slot = rp.NextAfter(slot);
        }
        tx.Unpin(blk);
        tx.Commit();
    }
}
