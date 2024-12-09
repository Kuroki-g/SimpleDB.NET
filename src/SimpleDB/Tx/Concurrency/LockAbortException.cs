namespace SimpleDB.Tx.Concurrency;

public class LockAbortException : SystemException
{
    public LockAbortException()
        : base() { }

    public LockAbortException(string message)
        : base(message) { }
}
