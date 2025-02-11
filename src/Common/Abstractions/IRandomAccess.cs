using Microsoft.Win32.SafeHandles;

namespace Common.Abstractions;

/// <summary>
/// RandomAccess の操作を表すインターフェース
/// </summary>
public interface IRandomAccess
{
    void Write(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset);
    void Read(SafeFileHandle handle, Span<byte> buffer, long fileOffset);
    long GetLength(SafeFileHandle handle);
}
