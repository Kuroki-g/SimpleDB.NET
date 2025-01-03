using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public class MetadataManager : IMetadataManager
{
    private static TableManager s_tableManager = null!;

    private static ViewManager s_viewManager = null!;

    private static StatManager s_statManager = null!;

    private static IndexManager s_indexManager = null!;

    public MetadataManager(bool isNew, ITransaction tx)
    {
        s_tableManager = new TableManager(isNew, tx);
        s_viewManager = new ViewManager(isNew, s_tableManager, tx);
        s_statManager = new StatManager(s_tableManager, tx);
        s_indexManager = new IndexManager(isNew, s_tableManager, s_statManager, tx);
    }

    public void CreateIndex(string indexName, string tableName, string fieldName, ITransaction tx)
    {
        s_indexManager.CreateIndex(tableName, tableName, fieldName, IndexType.HASH, tx);
    }

    public void CreateTable(string tableName, Schema schema, ITransaction tx)
    {
        s_tableManager.CreateTable(tableName, schema, tx);
    }

    public void CreateView(string viewName, string viewDef, ITransaction tx)
    {
        s_viewManager.CreateView(viewName, viewDef, tx);
    }

    public Dictionary<string, IndexInfo> GetIndexInfo(string tableName, ITransaction tx)
    {
        return s_indexManager.GetIndexInfo(tableName, tx);
    }

    public Layout GetLayout(string tableName, ITransaction tx)
    {
        return s_tableManager.GetLayout(tableName, tx);
    }

    public StatInfo GetStatInfo(string tableName, Layout layout, ITransaction tx)
    {
        return s_statManager.GetStatInfo(tableName, layout, tx);
    }

    public string GetViewDef(string viewName, ITransaction tx)
    {
        return s_viewManager.GetViewDef(viewName, tx);
    }
}
