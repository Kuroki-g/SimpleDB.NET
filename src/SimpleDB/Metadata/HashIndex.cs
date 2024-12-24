using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public class HashIndex : IIndex
{
    private ITransaction _tx;
    private string _indexName;
    private Layout _layout;

    public HashIndex(ITransaction tx, string indexName, Layout layout)
    {
        _tx = tx;
        _indexName = indexName;
        _layout = layout;
    }

    public static int SearchCost(int blockCount, int rpb)
    {
        throw new NotImplementedException();
    }
}
