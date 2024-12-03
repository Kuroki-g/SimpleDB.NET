using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace SimpleDB.Storage;

public sealed class FileManager
{
    private readonly DirectoryInfo _dbDirectory;

    public int BlockSize { get; private set; }

    public bool IsNew { get; private set; }

    public FileManager(string dbDirectory, int blocksize)
    {
        BlockSize = blocksize;
        _dbDirectory = new DirectoryInfo(dbDirectory);

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

    [MethodImpl(MethodImplOptions.Synchronized)]
    public BlockId Append(string fileName)
    {
        var info = new FileInfo(fileName);
        int newBlockNumber = (int)Math.Ceiling((double)info.Length / BlockSize);
        BlockId block = new(fileName, newBlockNumber);
        byte[] bytes = new byte[BlockSize];
        try
        {
            using FileStream fs = File.Create(info.FullName);
            fs.Seek(block.Number * BlockSize, SeekOrigin.Begin);
            fs.Write(bytes, 0, bytes.Length);
        }
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }

        return block;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Read(BlockId blockId, Page page)
    {
        try
        {
            using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(blockId.FileName, FileMode.OpenOrCreate);
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(blockId.Number * BlockSize, page.Contents().Length);
            accessor.ReadArray(0, page.Contents(), 0, page.Contents().Length);
        }
        // TODO: このエラーハンドリングが正しいか確認する。意味がない。
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
            using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(blockId.FileName, FileMode.OpenOrCreate);
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(blockId.Number * BlockSize, page.Contents().Length);
            accessor.WriteArray(0, page.Contents(), 0, page.Contents().Length);
        }
        // TODO: このエラーハンドリングが正しいか確認する。意味がない。
        catch (IOException e)
        {
            throw new SystemException(e.Message);
        }
    }

    public int Length(string fileName)
    {
        throw new NotImplementedException();
    }

}
