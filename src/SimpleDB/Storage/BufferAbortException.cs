namespace SimpleDB.Storage;

public class BufferAbortException : SystemException
{
    public BufferAbortException()
        : base() { }

    public BufferAbortException(string message)
        : base(message) { }

    public BufferAbortException(string message, Exception innerException)
        : base(message, innerException) { }
}
