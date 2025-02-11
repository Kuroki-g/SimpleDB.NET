using System.IO.Abstractions.TestingHelpers;
using SimpleDB.Storage;
using TestHelper.Utils;

namespace SimpleDB.Test.Storage;

public class FileManagerTest
{
    private const string _dir = "./mock";

    public FileManagerTest()
    {
        Helper.InitializeDir(_dir);
    }

    [Fact]
    public void FileManager_Instance_already_exist()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { @"./mock/sample", new MockFileData("sample file") },
            }
        );

        var manager = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = @"./mock",
                FileName = @"./mock/sample",
                BlockSize = 100,
            },
            fileSystem
        );

        Assert.Equal(100, manager.BlockSize);
        Assert.False(manager.IsNew);
    }

    [Fact]
    public void FileManager_Instance_new_directory()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
        var manager = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = @"./mock",
                FileName = @"./mock/sample",
                BlockSize = 100,
            },
            fileSystem
        );

        Assert.Equal(100, manager.BlockSize);
        Assert.True(manager.IsNew);
    }

    /// <summary>
    /// TODO: 統合テストのため移動したい。
    /// </summary>
    [Fact]
    public void Append_new_block()
    {
        string fileName = "test-file.blk";
        var blockSize = 400;
        var manager = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = @"./mock/sample",
                BlockSize = blockSize,
            }
        );

        var actual = manager.Append(fileName);

        Assert.Equal(fileName, actual.FileName);
    }

    /// <summary>
    /// TODO: 統合テストのため移動したい。
    /// </summary>
    [Fact]
    public void Append_multiple_time()
    {
        string fileName = "test-file.blk";
        var blockSize = 400;
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = @"./mock/sample",
                BlockSize = blockSize,
            }
        );
        Assert.Equal(0, fm.Length(fileName));

        var actual0 = fm.Append(fileName);
        Assert.Equal(0, actual0.Number);
        Assert.Equal(1, fm.Length(fileName));

        var actual1 = fm.Append(fileName);
        Assert.Equal(1, actual1.Number);
        Assert.Equal(2, fm.Length(fileName));

        var actual2 = fm.Append(fileName);
        Assert.Equal(2, actual2.Number);
        Assert.Equal(3, fm.Length(fileName));
    }

    /// <summary>
    /// TODO: 統合テストのため移動したい。
    /// </summary>
    [Fact]
    public void Write_new_block()
    {
        string fileName = "test-file.blk";
        var blockSize = 400;
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = @"./mock/sample",
                BlockSize = blockSize,
            }
        );

        var blockId = new BlockId(fileName, 2);
        var pageToWrite = new Page(fm.BlockSize);
        int initialPos = 0xA0;
        var str = "abcdefghijklm";
        pageToWrite.SetString(initialPos, str);
        int size = Page.MaxLength(str.Length);
        int pos2 = initialPos + size;
        pageToWrite.SetInt(pos2, 345);

        fm.Write(blockId, pageToWrite);
        var pageToRead = new Page(fm.BlockSize);
        fm.Read(blockId, pageToRead);

        Assert.Equal(str, pageToRead.GetString(initialPos));
        Assert.Equal(345, pageToRead.GetInt(pos2));
    }

    [Fact]
    public void Read_no_block_file_throws_exception()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
        var fm = FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = @"./mock/sample",
                BlockSize = 100,
            },
            fileSystem
        );

        var blockId = new BlockId("not-exist-block", 1);

        var actual = Record.Exception(() => fm.Read(blockId, new Page([])));

        Assert.IsType<SystemException>(actual);
    }
}
