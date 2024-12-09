using FakeItEasy;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Test.DataBuffer;

public class BufferAbortExceptionTest
{
    [Fact]
    public void Constructor()
    {
        static void func()
        {
            throw new BufferAbortException();
        }

        var actual = Record.Exception(func);

        Assert.IsType<BufferAbortException>(actual);
    }

    [Fact]
    public void Constructor_with_message()
    {
        static void func()
        {
            throw new BufferAbortException("message");
        }

        var actual = Record.Exception(func);

        Assert.Equal("message", actual.Message);
    }
}
