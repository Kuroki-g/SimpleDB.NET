using System.IO.Abstractions.TestingHelpers;
using Common;
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
    public void TestEmptyLog()
    {
        var fileName = "test.log";
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = _dir, BlockSize = 400 }
        );
        var logManager = LogManager.GetInstance(fm, fileName);
        using var logEnumerator = logManager.GetEnumerator();

        var result = Record.Exception(() => logEnumerator.MoveNext());

        Assert.IsType<EndOfStreamException>(result);
    }

    [Fact]
    public void TestSingleBlock()
    {
        var fileName = "test.log";
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = _dir, BlockSize = 400 }
        );
        var logManager = LogManager.GetInstance(fm, fileName);

        // ログレコードを追加
        var logRecord1 = CreateLogRecord("record1");
        var logRecord2 = CreateLogRecord("record2");
        var logRecord3 = CreateLogRecord("record3");

        logManager.Append(logRecord1);
        logManager.Append(logRecord2);
        logManager.Append(logRecord3);

        // LogEnumeratorを使ってログを逆順に読み込む
        using var logEnumerator = logManager.GetEnumerator();

        Assert.True(logEnumerator.MoveNext());
        var current = logEnumerator.Current;
        Assert.Equal(logRecord3, current); // 内容を比較

        Assert.True(logEnumerator.MoveNext());
        Assert.Equal(logRecord2, logEnumerator.Current); // 内容を比較

        Assert.True(logEnumerator.MoveNext());
        Assert.Equal(logRecord1, logEnumerator.Current); // 内容を比較
        Assert.Throws<InvalidOperationException>(() => logEnumerator.MoveNext());
    }

    [Fact]
    public void TestMultipleBlocks()
    {
        var dir = "LogEnumeratorTest_MultipleBlocks";
        var fileName = "test.log";
        var fm = FileManager.GetInstance(
            new FileManagerConfig { DbDirectory = _dir, BlockSize = 200 }
        );
        var logManager = LogManager.GetInstance(fm, fileName);

        // 複数のブロックにまたがるようにログレコードを追加
        var logRecord1 = CreateLogRecord("record1");
        var logRecord2 = CreateLogRecord("record2");
        var logRecord3 = CreateLogRecord("record3");
        var logRecord4 = CreateLogRecord("record4");

        logManager.Append(logRecord1);
        logManager.Append(logRecord2);
        logManager.Append(logRecord3);
        logManager.Append(logRecord4);

        // LogEnumeratorを使ってログを逆順に読み込む
        using var logEnumerator = logManager.GetEnumerator();

        Assert.True(logEnumerator.MoveNext());
        var current = logEnumerator.Current;
        Assert.Equal(logRecord4, current); // 内容を比較

        Assert.True(logEnumerator.MoveNext());
        Assert.Equal(logRecord3, logEnumerator.Current); // 内容を比較

        Assert.True(logEnumerator.MoveNext());
        Assert.Equal(logRecord2, logEnumerator.Current); // 内容を比較
        Assert.True(logEnumerator.MoveNext());

        Assert.Equal(logRecord1, logEnumerator.Current); // 内容を比較

        // Assert.False(logEnumerator.MoveNext()); // もうレコードがないことを確認
        Assert.Throws<InvalidOperationException>(() => logEnumerator.MoveNext());
    }

    private static byte[] CreateLogRecord(string s)
    {
        int spos = 0;
        int npos = Bytes.Integer + Page.CHARSET.GetBytes(s).Length; // 文字列の長さ(int) + 文字列自体のバイト数
        var b = new byte[npos + Bytes.Integer];
        using var p = new Page(b);
        p.SetString(spos, s);
        //p.SetInt(npos, 0); //dummy
        return b;
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
