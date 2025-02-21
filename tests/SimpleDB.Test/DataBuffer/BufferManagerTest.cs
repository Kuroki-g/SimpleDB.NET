using FakeItEasy;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Test.DataBuffer;

public class BufferManagerTest : IDisposable
{
    [Fact]
    public void Available_初期化した場合そのままだとバッファの数だけ利用可能である()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bufferCount = 3;
        var bufferManager = BufferManager.GetInstance(fm, lm, bufferCount);

        var actual = bufferManager.Available();

        Assert.Equal(bufferCount, actual);
    }

    [Fact]
    public void Pin_Pinした回数だけAvailableが減る()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bufferCount = 2;
        var bufferManager = BufferManager.GetInstance(fm, lm, bufferCount);

        bufferManager.Pin(new BlockId("test-file", 0));
        bufferManager.Pin(new BlockId("test-file", 1));

        var actual = bufferManager.Available();

        Assert.Equal(0, actual);
    }

    [Fact]
    public void Pin_バッファの最大数以上Pinした場合例外となる()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bufferCount = 2;
        var bufferManager = BufferManager.GetInstance(fm, lm, bufferCount);

        bufferManager.Pin(new BlockId("test-file", 0));
        bufferManager.Pin(new BlockId("test-file", 1));

        var actual = Record.Exception(() => bufferManager.Pin(new BlockId("test-file", 2)));

        Assert.IsType<BufferAbortException>(actual);
    }

    [Fact]
    public void Unpin_Unpinすると利用可能なバッファが増える()
    {
        var fm = A.Fake<IFileManager>();
        var lm = A.Fake<ILogManager>();
        var bufferCount = 1;
        var bufferManager = BufferManager.GetInstance(fm, lm, bufferCount);

        var buffer0 = bufferManager.Pin(new BlockId("test-file", 0));
        bufferManager.Unpin(buffer0);

        var actual = bufferManager.Available();

        Assert.Equal(1, actual);
    }

    public void Dispose()
    {
        BufferManager.InitializeInstance();
        GC.SuppressFinalize(this);
    }
}
