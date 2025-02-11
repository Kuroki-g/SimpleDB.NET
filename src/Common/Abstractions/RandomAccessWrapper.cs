using Microsoft.Win32.SafeHandles;

namespace Common.Abstractions;

/// <summary>
/// RandomAccess クラスをラップする実装
/// </summary>
public class RandomAccessWrapper : IRandomAccess
{
    public void Write(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset)
    {
        RandomAccess.Write(handle, buffer, fileOffset);
    }

    public void Read(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
    {
        RandomAccess.Read(handle, buffer, fileOffset);
    }

    public long GetLength(SafeFileHandle handle)
    {
        return RandomAccess.GetLength(handle);
    }
}
