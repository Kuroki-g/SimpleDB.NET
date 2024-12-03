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

    [Fact]
    public void Append_new_block()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
        var blockSize = 100;
        var manager = new FileManager(@"./mock", blockSize, fileSystem);

        var actual = manager.Append("new-block");

        Assert.Equal("new-block", actual.FileName);
        Assert.Equal(blockSize, actual.Number);
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
}
