namespace SimpleDB.Storage;

public interface IBufferManager
{
    public Buffer Pin(BlockId blockId);

    public void Unpin(Buffer buffer);

    public int Available();

    public void FlushAll(int txNumber);
}
