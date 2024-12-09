namespace SimpleDB.Tx.Concurrency;

public enum LockType
{
    NONE = 0,
    SHARED = 1,
    EXCLUSIVE = 2,
}
