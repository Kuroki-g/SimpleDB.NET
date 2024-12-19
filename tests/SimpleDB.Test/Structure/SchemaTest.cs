using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Test.Structure;

public class SchemaTest
{
    [Fact]
    public void AddAll_can_copy_all_fields()
    {
        var first = new Schema();
        first.AddIntField("A1");
        first.AddIntField("A2");
        first.AddIntField("A3");

        var another = new Schema();
        another.AddAll(first);

        Assert.Equal(first.Fields, another.Fields);
    }

    [Fact]
    public void AddIntField()
    {
        var schema = new Schema();

        schema.AddIntField("A");

        var actual = schema.Fields.First();
        Assert.Equal("A", actual);
        Assert.Equal(Types.INTEGER, schema.Type("A"));
    }

    [Fact]
    public void AddStringField()
    {
        var schema = new Schema();

        schema.AddStringField("B", 9);

        var actual = schema.Fields.First();
        Assert.Equal("B", actual);
        Assert.Equal(Types.VARCHAR, schema.Type("B"));
    }

    [Fact]
    public void HasField_true_if_exist()
    {
        var schema = new Schema();
        schema.AddIntField("A");

        var actual = schema.HasField("A");

        Assert.True(actual);
    }

    [Fact]
    public void HasField_false_if_not_exist()
    {
        var schema = new Schema();
        schema.AddIntField("A");

        var actual = schema.HasField("not-exist");

        Assert.False(actual);
    }
}
