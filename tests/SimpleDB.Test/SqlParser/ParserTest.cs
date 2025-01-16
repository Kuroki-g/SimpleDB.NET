using SimpleDB.SqlParser;

namespace SimpleDB.Test.SqlParser;

public class ParserTest
{
    [Fact]
    public void TestQuery()
    {
        var s = "select field1, field2 from table1 where field1 = 'value1'";
        var p = new Parser(s);

        var query = p.Query();

        Assert.NotNull(query);
        Assert.Equal(2, query.Fields.Count);
        Assert.Equal("field1", query.Fields[0]);
        Assert.Equal("field2", query.Fields[1]);
        Assert.Equal("table1", query.Tables.First());
        Assert.NotNull(query.Predicate);
    }

    [Fact]
    public void TestUpdateCmd_Insert()
    {
        var s = "insert into table1 (field1, field2) values ('value1', 123)";
        var p = new Parser(s);

        var cmd = p.UpdateCmd();

        Assert.IsType<Insert>(cmd);
        var insertCmd = (Insert)cmd;
        Assert.Equal("table1", insertCmd.Table);
        Assert.Equal(2, insertCmd.Fields.Count);
        Assert.Equal("field1", insertCmd.Fields[0]);
        Assert.Equal("field2", insertCmd.Fields[1]);
        Assert.Equal(2, insertCmd.Values.Count);
    }

    [Fact]
    public void TestUpdateCmd_Delete()
    {
        var s = "delete from table1 where field1 = 'value1'";
        var p = new Parser(s);
        var cmd = p.UpdateCmd();
        Assert.IsType<Delete>(cmd);
        var deleteCmd = (Delete)cmd;
        Assert.Equal("table1", deleteCmd.Table);
        Assert.NotNull(deleteCmd.Predicate);
    }
}
