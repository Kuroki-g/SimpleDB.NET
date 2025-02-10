using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Structure;

public class TableScanTest : IntegrationTestBase
{
    [Fact]
    public void TableScan_can_insert_int_to_table()
    {
        var tx = CreateTransaction();
        var schema = new Schema();
        var targetField = "int-field";
        schema.AddIntField(targetField);

        var ts = new TableScan(tx, "SampleTable", new Layout(schema));

        var random = new Random();
        var expectedInts = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            var a = random.Next();

            ts.Insert();
            ts.SetInt(targetField, a);
            expectedInts.Add(a);
        }
        ts.Close();
        tx.Commit();

        var actualInts = new List<int>();
        ts.BeforeFirst();
        while (ts.Next())
        {
            actualInts.Add(ts.GetInt(targetField));
        }

        Assert.Equal(expectedInts, actualInts);
    }

    [Fact]
    public void TableScan_can_insert_string_to_table()
    {
        var tx = CreateTransaction();
        var schema = new Schema();
        var targetField = "string-field";
        schema.AddStringField(targetField, 9);

        var ts = new TableScan(tx, "SampleTable", new Layout(schema));

        var expectedStrings = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            ts.Insert();

            var b = $"rec{_random.Next()}";
            ts.SetString(targetField, b);
            expectedStrings.Add(b);
        }
        ts.Close();
        tx.Commit();

        var actualStrings = new List<string>();
        ts.BeforeFirst();
        while (ts.Next())
        {
            var b = ts.GetString(targetField);
            actualStrings.Add(b);
        }

        Assert.Equal(expectedStrings, actualStrings);
    }

    [Fact]
    public void TableScan_can_insert_record_to_empty_table()
    {
        var targetField = "string-field";
        {
            // 空のテーブルを作成する。トランザクションをCommitまで行い、永続化する。
            using var tx = CreateTransaction();
            var _ = new TableManager(true, tx);

            var schema = new Schema();
            schema.AddStringField(targetField, 9);
            _.CreateTable("SampleTable", schema, tx);

            // var ts = new TableScan(tx, "SampleTable", new Layout(schema));
            tx.Commit();
        }
        // テーブル名を指定し、インサートを行う。
        using var tx2 = CreateTransaction();
        var tm = new TableManager(false, tx2);
        // ここで対象のスキーマが取れていない。
        var layout = tm.GetLayout("SampleTable", tx2);
        var ts2 = new TableScan(tx2, "SampleTable", layout);

        var expectedStrings = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            ts2.Insert();

            var b = $"rec{_random.Next()}";
            ts2.SetString(targetField, b);
            expectedStrings.Add(b);
        }
        ts2.Close();
        tx2.Commit();

        var actualStrings = new List<string>();
        ts2.BeforeFirst();
        while (ts2.Next())
        {
            var b = ts2.GetString(targetField);
            actualStrings.Add(b);
        }

        Assert.Equal(expectedStrings, actualStrings);
    }
}
