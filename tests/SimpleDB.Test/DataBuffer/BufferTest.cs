using FakeItEasy;
using SimpleDB.Logging;
using SimpleDB.Storage;
using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Test.DataBuffer;

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

    [Fact]
    public void Block_is_null_without_AssignToBlock()
    {
        var fm = A.Fake<IFileManager>();
        A.CallTo(() => fm.BlockSize).Returns(4);
        var lm = A.Fake<ILogManager>();
        var buffer = new Buffer(fm, lm);

        var actual = buffer.Contents.Contents().Length;

        Assert.Null(buffer.Block);
    }

    [Fact]
    public void Block_AssignToBlock_requied_to_set_block()
    {
        var fm = A.Fake<IFileManager>();
        A.CallTo(() => fm.BlockSize).Returns(0x0F);
        var lm = A.Fake<ILogManager>();
        var buffer = new Buffer(fm, lm);
        var blockId = new BlockId("test-block", 0x0F);

        buffer.AssignToBlock(blockId);

        Assert.True(blockId.Equals(buffer.Block));
    }

    [Fact]
    public void Flush_write_via_file_manager()
    {
        var fm = A.Fake<IFileManager>();
        A.CallTo(() => fm.BlockSize).Returns(4);
        var lm = A.Fake<ILogManager>();
        var blockId = new BlockId("test-block", 0x0F);
        var buffer = new Buffer(fm, lm);

        buffer.AssignToBlock(blockId);
        buffer.SetModified(1, 0);
        buffer.Flush();

        A.CallTo(() => lm.Flush(A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fm.Write(A<BlockId>._, A<Page>._)).MustHaveHappenedOnceExactly();
    }
}
