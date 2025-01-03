using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Parser.Grammar;

public interface IPlan
{
    public IScan Open();

    public Schema Schema { get; }

    public int BlocksAccessed { get; }
    public int RecordsOutput { get; }
    public int DistinctValues(string fieldName);
}
