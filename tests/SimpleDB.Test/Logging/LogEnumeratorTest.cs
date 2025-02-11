using System.IO.Abstractions.TestingHelpers;
using SimpleDB.Logging;
using SimpleDB.Storage;
using TestHelper.Utils;

namespace SimpleDB.Test.Logging;

public class LogEnumeratorTest
{
    private const string _dir = "./mock";

    public LogEnumeratorTest()
    {
        Helper.InitializeDir(_dir);
    }

    [Fact]
    public void Current_no_file()
    {
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = "./mock", BlockSize = 0x80 }
        );

        var blockId = new BlockId("log-file", 1);
        var enumerator = new LogEnumerator(fm, blockId);

        Assert.Empty(enumerator.Current);
    }

    [Fact]
    public void Current_has_file()
    {
        var fileName = "log-file";
        var blockId = new BlockId(fileName, 1);
        var blockSize = 0x80;
        CreateLogRecord(_dir, blockId, blockSize);

        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { @"./mock/sample", new MockFileData("sample file") },
            }
        );
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = "./mock", BlockSize = blockSize },
            fileSystem
        );
        var enumerator = new LogEnumerator(fm, blockId);

        var c0 = enumerator.Current;
        enumerator.MoveNext();
        var c1 = enumerator.Current;
        Assert.Empty(enumerator.Current);
    }

    [Fact(Skip = "TODO")]
    public void MoveNext()
    {
        var fileName = "log-file";
        var blockId = new BlockId(fileName, 1);
        var blockSize = 0x80;
        CreateLogRecord(_dir, blockId, blockSize);
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = "./mock", BlockSize = blockSize }
        );
        var enumerator = new LogEnumerator(fm, blockId);

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Length == 0)
                continue;
            var p = new Page(enumerator.Current);
            var contents = p.Contents();
            var s = p.GetString(0);
            int npos = Page.MaxLength(s.Length);
            int val = p.GetInt(npos);
            Assert.Equal("", s);
        }
        var last = enumerator.Current;
        var p2 = new Page(enumerator.Current);
        var contents2 = p2.Contents();
        var s2 = p2.GetString(0);
        int npos2 = Page.MaxLength(s2.Length);
        int val2 = p2.GetInt(npos2);
        Assert.Equal("", s2);
    }

    private static void CreateLogRecord(string dir, BlockId blockId, int blockSize)
    {
        Helper.InitializeDir(dir);
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = "./mock", BlockSize = blockSize }
        );
        var record = LoggingTestHelper.CreateLogRecord("record1", 1);
        var pageToWrite = new Page(record);

        fm.Write(blockId, pageToWrite);
        fm.Dispose();
    }
}
