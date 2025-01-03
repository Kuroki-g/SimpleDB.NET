using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface IMetadataManager
{
    public void CreateTable(string tableName, Schema schema, ITransaction tx);

    public Layout GetLayout(string tableName, ITransaction tx);

    public void CreateView(string viewName, string viewDef, ITransaction tx);

    public string GetViewDef(string viewName, ITransaction tx);

    public void CreateIndex(string indexName, string tableName, string fieldName, ITransaction tx);

    public Dictionary<string, IndexInfo> GetIndexInfo(string indexName, ITransaction tx);

    public StatInfo GetStatInfo(string tableName, Layout layout, ITransaction tx);
}
