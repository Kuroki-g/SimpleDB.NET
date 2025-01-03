using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

internal interface ITableManager
{
    /// <summary>
    /// the max chars in a view definition.
    /// </summary>
    public int MAX_NAME { get; }

    public void CreateTable(string tableName, Schema schema, ITransaction tx);

    public Layout GetLayout(string tableName, ITransaction tx);
}
