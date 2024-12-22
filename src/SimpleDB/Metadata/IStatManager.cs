using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public interface IStatManager
{
    public StatInfo GetStatInfo(string tableName, Layout layout, ITransaction tx);
}
