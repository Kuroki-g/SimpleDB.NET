namespace SimpleDB.Metadata;

public interface IIndex
{
    static abstract int SearchCost(int blockCount, int rpb);
}

public enum IndexType
{
    HASH,
    BTREE,
}
