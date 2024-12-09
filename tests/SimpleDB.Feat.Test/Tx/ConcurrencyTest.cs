using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Tx;

public class ConcurrencyTest : IntegrationTestBase
{
    /// https://zenn.dev/higty/articles/fea5f57cd1b1c2
    [Fact]
    public void ConcurrencyTestWithSingleTread()
    {
        var blockSize = 400;
        var bufferCount = 3;
        var logFile = "log-file";
        var fm = new FileManager(_dir, blockSize);
        var lm = new LogManager(fm, logFile);
        var bm = new BufferManager(fm, lm, bufferCount);
        var txA = new Transaction(fm, lm, bm);
        var blk1 = new BlockId("testfile", 1);
        var blk2 = new BlockId("testfile", 2);
        txA.Pin(blk1);
        txA.Pin(blk2);
        // System.out.println("Tx A: request slock 1");
        txA.GetInt(blk1, 0);
        // System.out.println("Tx A: receive slock 1");
        // Thread.sleep(1000);
        // System.out.println("Tx A: request slock 2");
        txA.GetInt(blk2, 0);
        // System.out.println("Tx A: receive slock 2");
        txA.Commit();
        // System.out.println("Tx A: commit");
    }
}
