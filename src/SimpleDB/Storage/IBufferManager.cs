using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Storage;

public interface IBufferManager : IDisposable
{
    public Buffer Pin(BlockId blockId);

    public void Unpin(Buffer buffer);

    public int Available();

    public void FlushAll(int txNumber);
}
