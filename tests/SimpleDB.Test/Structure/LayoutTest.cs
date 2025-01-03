using FakeItEasy;
using SimpleDB.Structure;

namespace SimpleDB.Test.Structure;

public class LayoutTest
{
    [Fact]
    public void Layout_hold_passed_schema()
    {
        var schema = A.Fake<Schema>();

        var layout = new Layout(schema);

        Assert.True(schema.Equals(layout.Schema));
    }
}
