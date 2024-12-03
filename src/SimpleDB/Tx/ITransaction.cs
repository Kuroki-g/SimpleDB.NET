using SimpleDB.Storage;

namespace SimpleDB.Tx;

public interface ITransaction
{
    public void Commit();

    public void Rollback();

    public void Recover();

    public void Pin(BlockId blockId);

    public void Unpin(BlockId blockId);

    public int GetInt(BlockId blockId, int offset);

    public string GetString(BlockId blockId, int offset);

    public int SetInt(BlockId blockId, int offset, int value, bool okToLog);

    public int SetString(BlockId blockId, int offset, string value, bool okToLog);

    public int AvailableBuffers();

    public int Size(string fileName);

    public BlockId Append(string fileName);

    public int BlockSize();
}
