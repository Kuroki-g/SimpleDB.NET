using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace SimpleDB.Storage;

internal sealed class FileManager : IFileManager, IDisposable
{
    readonly IFileSystem _fileSystem;

    private readonly IDirectoryInfo _dbDirectory;

    private readonly Dictionary<string, SafeFileHandle> _openFiles = [];

    internal string[] OpenedFiles => [.. _openFiles.Keys];

    public int BlockSize { get; private set; }

    public bool IsNew { get; private set; }

    public FileManager(string dbDirectory, int blockSize, IFileSystem? fileSystem = null)
    {
        if (blockSize <= 0)
            throw new InvalidOperationException("block size must be larger than zero");
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
        int newBlockNumber = Length(fileName);
        var blockId = new BlockId(fileName, newBlockNumber);
        byte[] bytes = new byte[BlockSize];
        try
        {
            var handle = GetFile(blockId.FileName);
            RandomAccess.Write(handle, new byte[bytes.Length], blockId.Number * BlockSize);
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
            var handle = GetFile(blockId.FileName);
            var bytes = new byte[BlockSize];
            var buffer = new Span<byte>(bytes);
            RandomAccess.Read(handle, buffer, blockId.Number * BlockSize);
            page.SetContents(buffer);
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
            var handle = GetFile(blockId.FileName);
            RandomAccess.Write(handle, page.Contents(), blockId.Number * BlockSize);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }
    }

    public int Length(string fileName)
    {
        var handle = GetFile(fileName) ?? throw new IOException($"cannot access {fileName}");
        return (int)(RandomAccess.GetLength(handle) / BlockSize);
    }

    /// <summary>
    /// Get handle or add new
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private SafeFileHandle GetFile(string filename)
    {
        var info = _fileSystem.FileInfo.New(
            _fileSystem.Path.Combine(_dbDirectory.FullName, filename)
        );
        SafeFileHandle? handle = _openFiles.GetValueOrDefault(info.FullName) ?? null;
        if (handle is not null)
        {
            return handle;
        }
        handle = CreateOrSelectFileAndOpenHandle(info, access: FileAccess.ReadWrite);
        _openFiles[info.FullName] = handle;
        return handle;
    }

    private static SafeFileHandle CreateOrSelectFileAndOpenHandle(
        IFileInfo info,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read,
        FileOptions options = FileOptions.None,
        long preallocationSize = 0
    )
    {
        if (!Directory.Exists(info.DirectoryName))
        {
            throw new FileNotFoundException(
                "cannot use this method for mock. please check you are using real file system."
            );
        }
        var fs = info.Exists ? info.Open(FileMode.Open) : info.Create();
        fs.Close();
        return File.OpenHandle(info.FullName, mode, access, share, options, preallocationSize);
    }

    public void Dispose()
    {
        foreach (var pair in _openFiles)
        {
            pair.Value.Dispose();
        }
    }
}
