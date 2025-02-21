using System.Runtime.CompilerServices;

namespace SimpleDB.Storage;

public class FileManagerConfig
{
    public string DbDirectory { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public int BlockSize { get; init; }
}

/// <summary>
/// WARNING: implement IDisposable when you use this interface
/// </summary>
public interface IFileManager : IDisposable
{
    public int BlockSize { get; }

    public bool IsNew { get; }

    /// <summary>
    /// 指定のファイル名のブロックを追加し、初期化する。
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public BlockId Append(string fileName);

    public void Read(BlockId blockId, Page page);

    public void Write(BlockId blockId, Page page);

    public int GetFileLength(string fileName);
}
