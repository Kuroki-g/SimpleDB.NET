namespace SimpleDB.Tx.Recovery;

public enum TransactionStatus
{
    CHECKPOINT = 0,
    START = 1,
    COMMIT = 2,
    ROLLBACK = 3,
    SETINT = 4,
    SETSTRING = 5,
}
