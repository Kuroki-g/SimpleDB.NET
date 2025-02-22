using SimpleDB.DataBuffer;

namespace SimpleDB.Test.DataBuffer;

public class BufferAbortExceptionTest
{
    [Fact]
    public void Constructor()
    {
        static void Func()
        {
            throw new BufferAbortException();
        }

        var actual = Record.Exception(Func);

        Assert.IsType<BufferAbortException>(actual);
    }

    [Fact]
    public void Constructor_with_message()
    {
        static void Func()
        {
            throw new BufferAbortException("message");
        }

        var actual = Record.Exception(Func);

        Assert.Equal("message", actual.Message);
    }
}
