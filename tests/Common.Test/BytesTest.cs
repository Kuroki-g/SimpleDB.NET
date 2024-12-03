using Common;

namespace Common.Test;

public class BytesTest
{
    [Fact]
    public void Integer()
    {
        Assert.Equal(4, Bytes.Integer);
    }

    [Fact]
    public void Long()
    {
        Assert.Equal(8, Bytes.Long);
    }
}
