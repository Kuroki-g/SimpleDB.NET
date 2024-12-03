namespace SimpleDB.Tx.Recovery;

public interface IRecordFactory
{
    public ILogRecord Create(byte[] bytes);
}
