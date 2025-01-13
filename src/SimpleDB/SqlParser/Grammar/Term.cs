using SimpleDB.Sql;
using SimpleDB.Structure;

namespace SimpleDB.SqlParser.Grammar;

public class Term(Expression lhs, Expression rhs)
{
    private readonly Expression _lhs = lhs;

    private readonly Expression _rhs = rhs;

    public bool IsSatisfied(IScan scan)
    {
        return _lhs.Evaluate(scan).Equals(_rhs.Evaluate(scan));
    }

    public Constant? EquatesWithConstant(string fieldName)
    {
        // _lhs
        var isEqualsLhs = _lhs.IsFieldName && _lhs.AsFieldName!.Equals(fieldName);
        if (isEqualsLhs && !_rhs.IsFieldName)
        {
            return _rhs.AsConstant;
        }
        // _rhs
        var isEqualsRhs = _rhs.IsFieldName && _rhs.AsFieldName!.Equals(fieldName);
        if (isEqualsRhs && !_lhs.IsFieldName)
        {
            return _lhs.AsConstant;
        }

        return null;
    }

    public string? EquatesWithField(string fieldName)
    {
        // _lhs
        var isEqualsLhs = _lhs.IsFieldName && _lhs.AsFieldName!.Equals(fieldName);
        if (isEqualsLhs && _rhs.IsFieldName)
        {
            return _rhs.AsFieldName;
        }
        // _rhs
        var isEqualsRhs = _rhs.IsFieldName && _rhs.AsFieldName!.Equals(fieldName);
        if (isEqualsRhs && _lhs.IsFieldName)
        {
            return _lhs.AsFieldName;
        }

        return null;
    }

    public int ReductionFactor(IPlan plan)
    {
        throw new NotImplementedException();
    }

    public bool AppliesTo(Schema schema)
    {
        return _lhs.AppliesTo(schema) && _rhs.AppliesTo(schema);
    }

    public override string ToString()
    {
        return $"{_lhs} = {_rhs}";
    }
}
