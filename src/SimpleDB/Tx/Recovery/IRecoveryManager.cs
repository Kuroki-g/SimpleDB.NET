using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Tx.Recovery;

public interface IRecoveryManager
{
    public void Commit();

    public void Rollback();

    public void Recover();

    public int SetInt(Buffer buffer, int offset, int newValue);

    public int SetString(Buffer buffer, int offset, string newValue);
}
