namespace SimpleDB.Storage;

public interface IBuffer
{
    public Page Contents { get; }

    public BlockId BlockId { get; }

    public bool IsPinned { get; }

    public void SetModified();

    public int ModifiyingTx();
}
