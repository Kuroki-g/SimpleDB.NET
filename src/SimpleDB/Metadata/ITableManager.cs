using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface ITableManager
{
    /// <summary>
    /// the max chars in a view definition.
    /// </summary>
    public int MAX_NAME { get; }

    public void CreateTable(string tableName, ISchema schema, ITransaction tx);

    public Layout GetLayout(string tableName, ITransaction tx);
}
