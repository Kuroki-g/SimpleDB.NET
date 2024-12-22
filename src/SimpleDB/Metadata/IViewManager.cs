using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface IViewManager
{
    public void CreateView(string viewName, string viewDef, ITransaction tx);

    public string GetViewDef(string viewName, ITransaction tx);
}
