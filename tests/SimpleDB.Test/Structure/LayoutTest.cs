using System.Collections.ObjectModel;
using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Test.Structure;

public class LayoutTest
{
    [Fact]
    public void Fields_can_add_field()
    {
        var schema = new Schema();
        schema.AddIntField("A");
        schema.AddStringField("B", 9);
        var expected = new ReadOnlyCollection<string>(["A", "B"]);

        var actual = schema.Fields;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fields_INTEGER()
    {
        var schema = new Schema();
        schema.AddIntField("A");

        var layout = new Layout(schema);
        Assert.Equal("A", layout.Schema.Fields[0]);
        Assert.Equal(Types.INTEGER, layout.Offset("A"));
    }

    [Fact]
    public void Fields_VARCHAR()
    {
        var schema = new Schema();
        schema.AddStringField("B", 9);

        var layout = new Layout(schema);
        Assert.Equal("B", layout.Schema.Fields[0]);
        Assert.Equal(Types.VARCHAR, layout.Offset("B"));
    }

    [Fact]
    public void Fields_multiple_fields()
    {
        var schema = new Schema();
        schema.AddIntField("A");
        schema.AddStringField("B", 9);
        var layout = new Layout(schema);

        Assert.Equal(2, layout.Schema.Fields.Count);
        Assert.Equal("A", layout.Schema.Fields[0]);
        Assert.Equal("B", layout.Schema.Fields[1]);
    }
}
