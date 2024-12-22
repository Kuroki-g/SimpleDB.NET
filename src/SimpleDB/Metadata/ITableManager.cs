using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface ITableManager
{
    public void CreateTable(string tableName, ISchema schema, ITransaction tx);

    public Layout GetLayout(string tableName, ITransaction tx);
}
