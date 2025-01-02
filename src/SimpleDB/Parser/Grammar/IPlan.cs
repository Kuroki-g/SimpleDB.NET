using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Parser.Grammar;

public interface IPlan
{
    public IScan Open();

    public ISchema Schema { get; }

    public int BlocksAccessed { get; }
    public int RecordsOutput { get; }
    public int DistinctValues(string fieldName);
}
