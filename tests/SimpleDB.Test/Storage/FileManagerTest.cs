using System.IO.Abstractions.TestingHelpers;
using SimpleDB.Storage;

namespace SimpleDB.Test.Storage;

public class FileManagerTest
{
    [Fact]
    public void FileManager_Instance_already_exist()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { @"./mock/sample", new MockFileData("sample file") },
            }
        );
        var manager = new FileManager(@"./mock", 100, fileSystem);

        Assert.Equal(100, manager.BlockSize);
        Assert.False(manager.IsNew);
    }

    [Fact]
    public void FileManager_Instance_new_directory()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
        var manager = new FileManager(@"./mock", 100, fileSystem);

        Assert.Equal(100, manager.BlockSize);
        Assert.True(manager.IsNew);
    }

    /// <summary>
    /// TODO: 統合テストのため移動したい。
    /// </summary>
    [Fact]
    public void Append_new_block()
    {
        var dbDirectory = @"./mock";
        string fileName = "test-file.blk";
        CleanUp(dbDirectory, fileName);

        var blockSize = 400;
        var manager = new FileManager(dbDirectory, blockSize);

        var actual = manager.Append(fileName);

        Assert.Equal(fileName, actual.FileName);

        CleanUp(dbDirectory, fileName);
    }

    /// <summary>
    /// TODO: 統合テストのため移動したい。
    /// </summary>
    [Fact]
    public void Write_new_block()
    {
        var dbDirectory = @"./mock";
        string fileName = "test-file.blk";
        CleanUp(dbDirectory, fileName);
        var blockSize = 400;
        var fm = new FileManager(dbDirectory, blockSize);

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

        CleanUp(dbDirectory, fileName);
    }

    [Fact]
    public void Read_no_block_file_throws_exception()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
        var manager = new FileManager(@"./mock", 100, fileSystem);
        var blockId = new BlockId("not-exist-block", 1);

        var actual = Record.Exception(() => manager.Read(blockId, new Page([])));

        Assert.IsType<SystemException>(actual);
    }

    private static void CleanUp(string dbDirectory, string fileName)
    {
        // Clean up
        var info = new FileInfo(Path.Combine(dbDirectory, fileName));
        if (info.Exists)
            info.Delete();
    }
}
