using FakeItEasy;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx;

namespace SimpleDB.Feat.Test.Tx;

public class BufferBoardTest : IntegrationTestBase
{
    [Fact]
    public void GetBuffer_return_null()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bm = new BufferManager(fm, lm, 3);
        var board = new BufferBoard(bm);

        var blockId = new BlockId("block-file", 1);
        var actual = board.GetBuffer(blockId);

        Assert.Null(actual);
    }

    [Fact]
    public void Pin_and_GetBuffer()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bm = new BufferManager(fm, lm, 3);
        var board = new BufferBoard(bm);

        var blockId = new BlockId("block-file", 1);
        board.Pin(blockId);
        var buffer = board.GetBuffer(blockId);

        Assert.True(buffer?.Block?.Equals(blockId));
    }

    [Fact]
    public void Pin_and_Unpin_and_GetBuffer_return_null()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bm = new BufferManager(fm, lm, 3);
        var board = new BufferBoard(bm);

        var blockId = new BlockId("block-file", 1);
        board.Pin(blockId);
        board.Unpin(blockId);
        var actual = board.GetBuffer(blockId);

        Assert.Null(actual);
    }
}
