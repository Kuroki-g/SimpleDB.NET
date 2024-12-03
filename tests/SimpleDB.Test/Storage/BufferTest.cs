using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
using Buffer = SimpleDB.Storage.Buffer;

namespace SimpleDB.Test.Storage;

public class BufferTest
{
    [Fact]
    public void Constructor_Page_length_set_from_file_manager()
    {
        var fm = A.Fake<IFileManager>();
        A.CallTo(() => fm.BlockSize).Returns(4);
        var lm = A.Fake<ILogManager>();
        var buffer = new Buffer(fm, lm);

        var actual = buffer.Contents.Contents().Length;

        Assert.Equal(4, actual);
    }
}
