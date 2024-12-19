using SimpleDB.Feat.Test.Tx;
using SimpleDB.Metadata;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Structure;

public class TableScanTest : IntegrationTestBase
{
    private readonly Random _random;

    public TableScanTest()
    {
        _random = new Random();
    }

    [Fact]
    public void TableScan_can_insert_int_to_table()
    {
        var tx = CreateTransaction();
        var schema = new Schema();
        var targetField = "int-field";
        schema.AddIntField(targetField);

        var layout = new Layout(schema);
        var ts = new TableScan(tx, "T", layout);

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

        var ts = new TableScan(tx, "tableName", new Layout(schema));

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
}
