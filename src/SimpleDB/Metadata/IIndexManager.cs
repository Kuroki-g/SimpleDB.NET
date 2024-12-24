using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface IIndexManager
{
    public void CreateIndex(
        string indexName,
        string tableName,
        string fieldName,
        IndexType type,
        ITransaction tx
    );

    public Dictionary<string, IndexInfo> GetIndexInfo(string tableName, ITransaction tx);
}
