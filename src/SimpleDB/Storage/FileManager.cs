using System.Collections.Concurrent;
using System.IO.Abstractions;
using Common.Abstractions;
using Microsoft.Win32.SafeHandles;

namespace SimpleDB.Storage;

internal sealed class FileManager : IFileManager, IDisposable
{
    private static Lazy<FileManager>? s_lazyInstance;

    private static readonly object InitLock = new();

    private readonly IFileSystem _fileSystem;

    private readonly IDirectoryInfo _dbDirectory;

    private readonly ConcurrentDictionary<string, SafeFileHandle> _openFiles = new(); // ConcurrentDictionary を使用

    public string[] OpenedFiles => [.. _openFiles.Keys];

    public int BlockSize { get; private set; }

    public bool IsNew { get; private set; }

    private readonly IRandomAccess _randomAccess;

    // Private constructor to prevent external instantiation
    private FileManager(
        string dbDirectory,
        int blockSize,
        IFileSystem? fileSystem = null,
        IRandomAccess? randomAccess = null
    )
    {
        if (blockSize <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(blockSize),
                "Block size must be larger than zero."
            );

        _fileSystem = fileSystem ?? new FileSystem();
        _randomAccess = randomAccess ?? new RandomAccessWrapper();
        _dbDirectory = _fileSystem.DirectoryInfo.New(dbDirectory);

        BlockSize = blockSize;

        IsNew = !_dbDirectory.Exists;
        if (IsNew)
        {
            _dbDirectory.Create();
        }

        // tempファイルの削除 (より安全な方法)
        foreach (var fileInfo in _dbDirectory.GetFiles())
        {
            if (IsTemporaryFile(fileInfo)) // 判定メソッド
            {
                try
                {
                    fileInfo.Delete();
                    //ログ出力(例)
                    Console.WriteLine($"Deleted temporary file: {fileInfo.FullName}");
                }
                catch (Exception ex)
                {
                    //ログ出力(例)
                    Console.WriteLine(
                        $"Error deleting temporary file {fileInfo.FullName}: {ex.Message}"
                    );
                }
            }
        }
    }

    /// <summary>
    /// ここにtempファイルかどうかの判定ロジックを実装。
    /// 例：ファイル名が"temp"で始まり、かつ、タイムスタンプが付与されている、など。
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private static bool IsTemporaryFile(IFileInfo fileInfo)
    {
        return fileInfo.Name.StartsWith("temp");
    }

    public static FileManager GetInstance(
        FileManagerConfig? config = null,
        IFileSystem? fileSystem = null,
        IRandomAccess? randomAccess = null
    )
    {
        lock (InitLock)
        {
            if (s_lazyInstance == null)
            {
                ArgumentNullException.ThrowIfNull(config);
                s_lazyInstance = new Lazy<FileManager>(
                    () =>
                        new FileManager(
                            config.DbDirectory,
                            config.BlockSize,
                            fileSystem,
                            randomAccess
                        )
                );
            }
            else if (config != null) // 設定が違う場合
            {
                // 警告
                Console.WriteLine(
                    "Warning: FileManager already initialized, ignoring configuration changes."
                );
            }

            return s_lazyInstance.Value;
        }
    }

    // ファイルごとのロックオブジェクトを管理する ConcurrentDictionary
    private readonly ConcurrentDictionary<string, object> _fileLocks = new();

    /// <summary>
    /// 指定のファイル名のブロックを追加し、初期化する。
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="SystemException"></exception>
    public BlockId Append(string fileName)
    {
        // ファイル名に対応するロックオブジェクトを取得または作成
        var fileLock = _fileLocks.GetOrAdd(fileName, _ => new object());

        lock (fileLock) // ファイル単位でロック
        {
            int newBlockNumber = GetFileLength(fileName); //Lengthメソッドを修正
            var blockId = new BlockId(fileName, newBlockNumber);
            byte[] bytes = new byte[BlockSize]; // 初期化されたバイト配列

            try
            {
                var handle = GetFileHandle(fileName); // SafeFileHandle を取得
                _randomAccess.Write(handle, bytes, blockId.Number * BlockSize);
            }
            catch (IOException e)
            {
                throw new SystemException($"Error appending to file {fileName}: {e.Message}", e);
            }

            return blockId;
        }
    }

    public void Read(BlockId blockId, Page page)
    {
        var fileLock = _fileLocks.GetOrAdd(blockId.FileName, _ => new object());
        lock (fileLock)
        {
            try
            {
                var handle = GetFileHandle(blockId.FileName);
                var bytes = new byte[BlockSize];
                _randomAccess.Read(handle, bytes, blockId.Number * BlockSize);
                page.SetContents(bytes);
            }
            catch (IOException e)
            {
                throw new SystemException(
                    $"Error reading block {blockId.Number} from file {blockId.FileName}: {e.Message}",
                    e
                );
            }
        }
    }

    public void Write(BlockId blockId, Page page)
    {
        var fileLock = _fileLocks.GetOrAdd(blockId.FileName, _ => new object());
        lock (fileLock)
        {
            try
            {
                var handle = GetFileHandle(blockId.FileName);
                _randomAccess.Write(handle, page.Contents(), blockId.Number * BlockSize);
            }
            catch (IOException e)
            {
                throw new SystemException(
                    $"Error writing to block {blockId.Number} in file {blockId.FileName}: {e.Message}",
                    e
                );
            }
        }
    }

    // SafeFileHandle を利用してファイル長を取得するメソッド (GetFileHandle からの呼び出し専用)
    public int GetFileLength(string fileName)
    {
        var handle = GetFileHandle(fileName) ?? throw new IOException($"cannot access {fileName}");
        return (int)(_randomAccess.GetLength(handle) / BlockSize);
    }

    // SafeFileHandle を取得/管理するメソッド
    private SafeFileHandle GetFileHandle(string fileName)
    {
        return _openFiles.AddOrUpdate(
            fileName,
            _ => CreateFileHandle(fileName),
            (_, existingHandle) =>
            {
                if (existingHandle.IsClosed)
                {
                    existingHandle.Dispose();
                    return CreateFileHandle(fileName);
                }
                return existingHandle;
            }
        );
    }

    private SafeFileHandle CreateFileHandle(string fileName)
    {
        var filePath = _fileSystem.Path.Combine(_dbDirectory.FullName, fileName);
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return CreateOrSelectFileAndOpenHandle(fileInfo, access: FileAccess.ReadWrite);
    }

    private static SafeFileHandle CreateOrSelectFileAndOpenHandle(
        IFileInfo info,
        FileMode mode = FileMode.OpenOrCreate, // OpenOrCreate に変更
        FileAccess access = FileAccess.ReadWrite,
        FileShare share = FileShare.Read, // 他のプロセスからの読み取りを許可
        FileOptions options = FileOptions.None,
        long preallocationSize = 0
    )
    {
        // ディレクトリが存在しない場合は例外ではなく、作成するように変更
        if (!Directory.Exists(info.DirectoryName))
        {
            Directory.CreateDirectory(info.DirectoryName!);
        }

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
                foreach (var handle in _openFiles.Values)
                {
                    handle.Dispose();
                }
                _openFiles.Clear();
            }
            _disposed = true;
        }
    }

    ~FileManager()
    {
        Dispose(false);
    }

#if DEBUG
    // テスト用にインスタンスをリセットするメソッド
    internal static void ResetInstanceForTesting()
    {
        lock (InitLock)
        {
            if (s_lazyInstance != null)
            {
                s_lazyInstance.Value.Dispose(); // 現在のインスタンスを破棄
                s_lazyInstance = null; // インスタンスを null にリセット
            }
        }
    }

#endif
}
