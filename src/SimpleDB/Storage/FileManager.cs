using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace SimpleDB.Storage;

internal sealed class FileManager : IFileManager, IDisposable
{
    private static FileManager? _instance;
    private static readonly object _lock = new();

    readonly IFileSystem _fileSystem;

    private readonly IDirectoryInfo _dbDirectory;

    private readonly Dictionary<string, SafeFileHandle> _openFiles = [];

    internal string[] OpenedFiles => [.. _openFiles.Keys];

    public int BlockSize { get; private set; }

    public bool IsNew { get; private set; }

    // Private constructor to prevent external instantiation
    private FileManager(string dbDirectory, int blockSize, IFileSystem? fileSystem = null)
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

    // Public static method to get the instance
    public static FileManager GetInstance(
        FileManagerConfig? config = null,
        IFileSystem? fileSystem = null
    )
    {
        // Double-checked locking for thread safety
        if (_instance == null)
        {
            ArgumentNullException.ThrowIfNull(config);
            lock (_lock)
            {
                _instance ??= new FileManager(config.DbDirectory, config.BlockSize, fileSystem);
            }
        }
        // すでに存在している場合はディレクトリとブロックサイズの変更は無視する。
        return _instance;
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
            var handle = GetFileHandle(blockId.FileName);
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
            var handle = GetFileHandle(blockId.FileName);
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
            var handle = GetFileHandle(blockId.FileName);
            RandomAccess.Write(handle, page.Contents(), blockId.Number * BlockSize);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }
    }

    public int Length(string fileName)
    {
        var handle = GetFileHandle(fileName) ?? throw new IOException($"cannot access {fileName}");
        return (int)(RandomAccess.GetLength(handle) / BlockSize);
    }

    /// <summary>
    /// Get handle or add new
    /// WARNING: do not forget to dispose this handle
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private SafeFileHandle GetFileHandle(string fileName)
    {
        SafeFileHandle? handle = _openFiles.GetValueOrDefault(fileName) ?? null;
        if (handle is not null)
        {
            if (handle.IsClosed) // handleが閉じられている場合にはアクセスできないので再度開く
            {
                _openFiles.Remove(fileName);
                return GetFileHandle(fileName);
            }
            return handle;
        }
        var info = _fileSystem.FileInfo.New(
            _fileSystem.Path.Combine(_dbDirectory.FullName, fileName)
        );
        handle = CreateOrSelectFileAndOpenHandle(info, access: FileAccess.ReadWrite);
        _openFiles[fileName] = handle;
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

    // Dispose pattern
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources (if any)
                foreach (var pair in _openFiles)
                {
                    pair.Value.Dispose();
                }
                _openFiles.Clear();
            }

            // Release unmanaged resources (if any)
            // (none in this case)

            _disposed = true;
        }
    }

    // Finalizer (if needed)
    ~FileManager()
    {
        Dispose(false);
    }
}
