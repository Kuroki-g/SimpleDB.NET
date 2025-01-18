using SimpleDB.Feat.Test.Tx;
using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Feat.Test.Sql;

public class SelectScanTest : IntegrationTestBase
{
    [Fact]
    public void TestSelectScan_WhenSelectingAllFields_ThenReturnsAllFields()
    {
        // Arrange
        CreateSampleTable("TestTable", CreateSchema());

        {
            using var tx = CreateTransaction();
            var schema = CreateSchema();
            var layout = new Layout(schema);
            var scan = new TableScan(tx, "TestTable", layout);
            scan.Insert();
            scan.SetInt("A", 1);
            scan.SetString("B", "Alice");
            scan.Insert();
            scan.SetInt("A", 2);
            scan.SetString("B", "Bob");
            tx.Commit();
        }

        {
            using var tx = CreateTransaction();
            var schema = CreateSchema();
            var layout = new Layout(schema);
            var scan = new TableScan(tx, "TestTable", layout);
            var term = new Term(new Expression("A"), new Expression(new Constant(1)));
            var predicate = new Predicate(term);
            var selectScan = new SelectScan(scan, predicate);

            // Act
            string[] fields = ["B"];
            var projectScan = new ProjectScan(selectScan, fields);

            // Assert
            List<string> actual = [];
            while (projectScan.Next())
            {
                actual.Add(projectScan.GetString("B"));
            }
            projectScan.Close();
            tx.Commit();

            Assert.Equal(["Alice"], actual);
        }
    }
}
