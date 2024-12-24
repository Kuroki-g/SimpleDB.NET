using System.ComponentModel.DataAnnotations;
using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public class IndexManager : IIndexManager
{
    private readonly ITableManager _tm;
    private readonly IStatManager _sm;
    private readonly Layout _layout;

    public IndexManager(
        bool isNew,
        ITableManager tableManager,
        IStatManager statManager,
        ITransaction tx
    )
    {
        _tm = tableManager;
        _sm = statManager;

        if (isNew)
        {
            var schema = new IndexCatalogSchema(
                tableManager.MAX_NAME,
                tableManager.MAX_NAME,
                tableManager.MAX_NAME
            );
            _tm.CreateTable(IndexCatalogSchema.TABLE_NAME_IDX_CATALOG, schema, tx);
        }
        _layout = _tm.GetLayout(IndexCatalogSchema.TABLE_NAME_IDX_CATALOG, tx);
    }

    public void CreateIndex(
        string indexName,
        string tableName,
        string fieldName,
        IndexType type,
        ITransaction tx
    )
    {
        var ts = new TableScan(tx, IndexCatalogSchema.TABLE_NAME_IDX_CATALOG, _layout);
        ts.Insert();
        // TODO: Move to indexSchema and implement static virtual to interface
        ts.SetString(IndexCatalogSchema.FIELD_INDEX_NAME, indexName);
        ts.SetString(IndexCatalogSchema.FIELD_TABLE_NAME, tableName);
        ts.SetString(IndexCatalogSchema.FIELD_FIELD_NAME, fieldName);
        ts.Close();
    }

    public Dictionary<string, IndexInfo> GetIndexInfo(string tableName, ITransaction tx)
    {
        var result = new Dictionary<string, IndexInfo>();
        var ts = new TableScan(tx, IndexCatalogSchema.TABLE_NAME_IDX_CATALOG, _layout);
        while (ts.Next())
        {
            if (ts.GetString(IndexCatalogSchema.FIELD_TABLE_NAME) == tableName)
            {
                var indexName = ts.GetString(IndexCatalogSchema.FIELD_INDEX_NAME);
                var fieldName = ts.GetString(IndexCatalogSchema.FIELD_FIELD_NAME);

                var layout = _tm.GetLayout(tableName, tx);
                var statInfo = _sm.GetStatInfo(tableName, layout, tx);
                var indexInfo = new IndexInfo(indexName, fieldName, layout.Schema, tx, statInfo);
                result[indexName] = indexInfo;
            }
        }
        ts.Close();
        return result;
    }
}
