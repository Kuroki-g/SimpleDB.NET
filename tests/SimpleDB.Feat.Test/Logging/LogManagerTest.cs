using SimpleDB.Feat.Test.Tx;
using SimpleDB.Logging;
using SimpleDB.Storage;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Logging;

public class LogManagerTest : IntegrationTestBase
{
    [Fact]
    public void Constructor_new_log_file_no_error()
    {
        var fileName = "log-file";

        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );

        var exception = Record.Exception(() => LogManager.GetInstance(fm, fileName));

        Assert.Equal([fileName], fm.OpenedFiles);
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_has_log_file_no_error()
    {
        var fileName = "log-file";

        // create log file
        var _fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );
        var _lm = LogManager.GetInstance(_fm, fileName);
        LoggingTestHelper.CreateSampleLogRecords(_lm, 1, 35);
        _fm.Dispose(); // disposeして参照を破棄する。

        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );

        var exception = Record.Exception(() => LogManager.GetInstance(fm, fileName));

        Assert.Null(exception);
    }

    [Fact]
    public void Append_new_log_file_no_error()
    {
        var fileName = "log-file";
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );

        var lm = LogManager.GetInstance(fm, fileName);
        var logRecord = LoggingTestHelper.CreateLogRecord("first-record", 1);
        var lsn = lm.Append(logRecord);

        Assert.Equal(1, lsn);
    }

    [Fact(Skip = "TODO")]
    public void Append_multiple_time()
    {
        var fileName = "log-file";
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 0x80,
            }
        );
        var lm = LogManager.GetInstance(fm, fileName);
        for (int i = 0; i < 100; i++)
        {
            var logRecord = LoggingTestHelper.CreateLogRecord($"record{i}", i);
            var lsn = lm.Append(logRecord);
        }

        Assert.Equal(1, lm.CurrentBlock.Number);
    }

    [Fact]
    public void GetEnumerator_empty_file()
    {
        var fileName = "log-file";
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );

        var lm = LogManager.GetInstance(fm, fileName);

        var actual = LoggingTestHelper.GetLogRecords(lm);

        Assert.Empty(actual);
    }

    [Fact(Skip = "TODO")]
    public void CreateSample_GetRecords()
    {
        var fileName = "log-file";
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = fileName,
                BlockSize = 400,
            }
        );

        var lm = LogManager.GetInstance(fm, fileName);

        var actual = LoggingTestHelper.CreateSampleLogRecords(lm, 1, 35);
        var actual2 = LoggingTestHelper.CreateSampleLogRecords(lm, 36, 70);
        lm.Flush(65);

        var actual3 = LoggingTestHelper.GetLogRecords(lm);

        Assert.Empty(actual3);
    }
}
