using SimpleDB.Sql;

namespace SimpleDB.SqlParser.Grammar;

public class Predicate
{
    private readonly List<Term> _terms = [];

    public Predicate() { }

    public Predicate(Term term)
    {
        _terms.Add(term);
    }

    public void Conjoin(Predicate predicate)
    {
        _terms.AddRange(predicate._terms);
    }

    public bool IsSatisfied(IScan scan)
    {
        return _terms.All(term => term.IsSatisfied(scan));
    }

    public int ReductionFactor(IPlan plan)
    {
        var factor = 1;
        foreach (var term in _terms)
        {
            factor *= term.ReductionFactor(plan);
        }
        return factor;
    }
}
