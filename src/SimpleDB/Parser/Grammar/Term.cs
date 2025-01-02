using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.Parser.Grammar;

public class Term(Expression lhs, Expression rhs)
{
    private readonly Expression _lhs = lhs;

    private readonly Expression _rhs = rhs;

    public bool IsSatisfied(IScan scan)
    {
        return _lhs.Evaluate(scan).Equals(_rhs.Evaluate(scan));
    }

    public int ReductionFactor(IPlan plan)
    {
        throw new NotImplementedException();
    }

    public bool AppliesTo(ISchema schema)
    {
        return _lhs.AppliesTo(schema) && _rhs.AppliesTo(schema);
    }

    public override string ToString()
    {
        return $"{_lhs} = {_rhs}";
    }
}
