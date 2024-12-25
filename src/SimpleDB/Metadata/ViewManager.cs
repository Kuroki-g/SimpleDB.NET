using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

internal class ViewManager : IViewManager
{
    private static readonly int MAX_VIEWDEF = 100;

    private readonly ITableManager _tableManager;

    public ViewManager(bool isNew, ITableManager tableManager, ITransaction tx)
    {
        _tableManager = tableManager;
        if (isNew)
        {
            var schema = new ViewSchema(_tableManager.MAX_NAME, MAX_VIEWDEF);
            _tableManager.CreateTable(ViewSchema.TABLE_NAME_VIEW_CATALOG, schema, tx);
        }
    }

    public void CreateView(string viewName, string viewDef, ITransaction tx)
    {
        var layout = _tableManager.GetLayout(ViewSchema.TABLE_NAME_VIEW_CATALOG, tx);
        var ts = new TableScan(tx, ViewSchema.TABLE_NAME_VIEW_CATALOG, layout);
        ts.Insert();
        ViewSchema.SetFields(ts, viewName, viewDef);
        ts.Close();
    }

    public string GetViewDef(string viewName, ITransaction tx)
    {
        var result = string.Empty;
        var layout = _tableManager.GetLayout(ViewSchema.TABLE_NAME_VIEW_CATALOG, tx);
        var ts = new TableScan(tx, ViewSchema.TABLE_NAME_VIEW_CATALOG, layout);
        while (ts.Next())
            if (ts.GetString(ViewSchema.FIELD_VIEW_NAME).Equals(viewName))
            {
                result = ts.GetString(ViewSchema.FIELD_VIEW_DEF);
                break;
            }
        ts.Close();
        return result;
    }
}
