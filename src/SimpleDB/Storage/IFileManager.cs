using System.Runtime.CompilerServices;

namespace SimpleDB.Storage;

public interface IFileManager
{
    public int BlockSize { get; }

    public bool IsNew { get; }

    /// <summary>
    /// 指定のファイル名のブロックを追加し、初期化する。
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public BlockId Append(string fileName);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Read(BlockId blockId, Page page);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Write(BlockId blockId, Page page);

    public int Length(string fileName);
}
