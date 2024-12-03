using System.IO.Abstractions;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace SimpleDB.Storage;

internal sealed class FileManager : IFileManager, IDisposable
{
    readonly IFileSystem _fileSystem;

    private readonly IDirectoryInfo _dbDirectory;

    private readonly Dictionary<string, MemoryMappedFile> _openFiles = [];

    public int BlockSize { get; private set; }

    public bool IsNew { get; private set; }

    public FileManager(string dbDirectory, int blockSize, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _dbDirectory = _fileSystem.DirectoryInfo.New(dbDirectory);

        BlockSize = blockSize;

        IsNew = !_dbDirectory.Exists;
        if (IsNew)
        {
            _dbDirectory.Create();
        }

        // TODO: 一度に削除するメソッドがあるはず。書き換える。
        // TODO: ログに出力するようにする。
        foreach (var fileInfo in _dbDirectory.GetFiles())
        {
            if (fileInfo.Name.StartsWith("temp"))
            {
                fileInfo.Delete();
            }
        }
    }

    /// <summary>
    /// 指定のファイル名のブロックを追加し、初期化する。
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="SystemException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public BlockId Append(string fileName)
    {
        int newBlockNumber = fileName.Length;
        var blockId = new BlockId(fileName, newBlockNumber);
        byte[] bytes = new byte[BlockSize];
        try
        {
            var mmf = GetFile(blockId.FileName);
            var steam = mmf.CreateViewStream();
            steam.Seek(blockId.Number * BlockSize, SeekOrigin.Begin);
            steam.Write(bytes, 0, bytes.Length);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }

        return blockId;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Read(BlockId blockId, Page page)
    {
        try
        {
            var mmf = GetFile(blockId.FileName);
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(
                blockId.Number * BlockSize,
                page.Contents().Length
            );
            accessor.ReadArray(0, page.Contents(), 0, page.Contents().Length);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Write(BlockId blockId, Page page)
    {
        try
        {
            using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(
                blockId.FileName,
                FileMode.OpenOrCreate
            );
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(
                blockId.Number * BlockSize,
                page.Contents().Length
            );
            accessor.WriteArray(0, page.Contents(), 0, page.Contents().Length);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }
    }

    public int Length(string fileName)
    {
        throw new NotImplementedException();
    }

    private MemoryMappedFile GetFile(string filename)
    {
        IFileInfo f = _fileSystem.FileInfo.New(filename);
        using var handle = File.OpenHandle("test.txt", access: FileAccess.ReadWrite);

        if (f.Exists)
        {
            return MemoryMappedFile.CreateFromFile(f.FullName);
        }

        var file = MemoryMappedFile.CreateFromFile(f.FullName);
        _openFiles.Add(filename, file);
        return file;
    }

    public void Dispose()
    {
        foreach (var pair in _openFiles)
        {
            pair.Value.Dispose();
        }
    }
}
