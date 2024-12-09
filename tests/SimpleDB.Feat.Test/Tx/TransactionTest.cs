using FakeItEasy;
using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Tx;

public class TransactionTest : IntegrationTestBase
{
    [Fact]
    public void Transaction_commit_okToLog_false()
    {
        var fm = new FileManager(_dir, 0x90);
        var lm = new LogManager(fm, "log-file");
        var bm = new BufferManager(fm, lm, 1);

        var transaction = new Transaction(fm, lm, bm);

        var blockId = new BlockId("block-file", 1);
        transaction.Pin(blockId);
        // Don't log initial block values.
        transaction.SetInt(blockId, 80, 1, false);
        transaction.SetString(blockId, 40, "one", false);
        transaction.Commit();
    }

    [Fact]
    public void Transaction_commit_okToLog_true()
    {
        var fm = new FileManager(_dir, 0x90);
        var lm = new LogManager(fm, "log-file");
        var bm = new BufferManager(fm, lm, 1);

        var transaction = new Transaction(fm, lm, bm);

        var blockId = new BlockId("block-file", 1);
        transaction.Pin(blockId);
        // Don't log initial block values.
        transaction.SetInt(blockId, 80, 1, false);
        transaction.SetString(blockId, 40, "one", false);
        transaction.Commit();

        var transaction2 = new Transaction(fm, lm, bm);
        transaction2.Pin(blockId);
        // Don't log initial block values.
        transaction2.SetInt(blockId, 80, 1 + 1, true);
        transaction2.SetString(blockId, 40, "two", true);
        transaction2.Commit();
    }
}
