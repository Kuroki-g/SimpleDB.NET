using System.Runtime.CompilerServices;

namespace SimpleDB.Storage;

public interface IFileManager
{
    public int BlockSize { get; }

    public bool IsNew { get; }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public BlockId Append(string fileName);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Read(BlockId blockId, Page page);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Write(BlockId blockId, Page page);
}
