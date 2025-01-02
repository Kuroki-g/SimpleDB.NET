using SimpleDB.Parser.Grammar;

namespace SimpleDB.Parser;

public class Parser(string s)
{
    private readonly Lexer _lexer = new(s);

    public string Field => _lexer.EatId();

    public Constant Constant =>
        _lexer.MatchStringConstant()
            ? new Constant(_lexer.EatStringConstant())
            : new Constant(_lexer.EatIntConstant());

    public Expression Expression =>
        _lexer.IsIdMatch ? new Expression(Field) : new Expression(Constant);

    public Term Term()
    {
        var left = Expression;
        _lexer.EatDelim('=');
        var right = Expression;
        return new Term(left, right);
    }

    public Predicate Predicate()
    {
        var predicate = new Predicate(Term());
        while (_lexer.MatchKeyword("and"))
        {
            _lexer.EatKeyword("and");
            predicate.Conjoin(Predicate());
        }
        return predicate;
    }

    public Query Query()
    {
        _lexer.EatKeyword("select");
        var fields = SelectList();
        _lexer.EatKeyword("from");
        var tables = TableList();
        var predicate = new Predicate();
        if (_lexer.MatchKeyword("where"))
        {
            _lexer.EatKeyword("where");
            predicate = Predicate();
        }
        return new Query(fields, tables, predicate);
    }

    private List<string> SelectList()
    {
        List<string> fields = [];
        fields.Add(Field);
        while (_lexer.MatchDelim(','))
        {
            _lexer.EatDelim(',');
            fields.AddRange(SelectList());
        }
        return fields;
    }

    private IEnumerable<string> TableList()
    {
        List<string> tables = [];
        tables.Add(_lexer.EatId());
        while (_lexer.MatchDelim(','))
        {
            _lexer.EatDelim(',');
            tables.AddRange(TableList());
        }
        return tables;
    }

    public object UpdateCmd()
    {
        if (_lexer.MatchKeyword("insert"))
        {
            return Insert();
        }
        else if (_lexer.MatchKeyword("delete"))
        {
            return Delete();
        }
        else if (_lexer.MatchKeyword("update"))
        {
            return Modify();
        }
        else
        {
            return Create();
        }
    }

    private object Create()
    {
        _lexer.EatKeyword("create");
        if (_lexer.MatchKeyword("table"))
        {
            return CreateTable();
        }
        else if (_lexer.MatchKeyword("view"))
        {
            return CreateView();
        }
        else
        {
            return CreateIndex();
        }
    }

    private object CreateIndex()
    {
        throw new NotImplementedException();
    }

    private object CreateView()
    {
        throw new NotImplementedException();
    }

    private object CreateTable()
    {
        throw new NotImplementedException();
    }

    private object Modify()
    {
        throw new NotImplementedException();
    }

    private Delete Delete()
    {
        _lexer.EatKeyword("delete");
        _lexer.EatKeyword("from");
        var table = _lexer.EatId();
        var predicate = new Predicate();
        if (_lexer.MatchKeyword("where"))
        {
            _lexer.EatKeyword("where");
            predicate = Predicate();
        }
        return new Delete(table, predicate);
    }

    private Insert Insert()
    {
        _lexer.EatKeyword("insert");
        _lexer.EatKeyword("into");
        var table = _lexer.EatId();
        _lexer.EatDelim('(');
        List<string> fields = FieldList();
        _lexer.EatDelim(')');
        _lexer.EatKeyword("values");
        _lexer.EatDelim('(');
        List<Constant> values = ConstList();
        _lexer.EatDelim(')');
        return new Insert(table, fields, values);
    }

    private List<Constant> ConstList()
    {
        List<Constant> list = [];
        list.Add(Constant);
        if (_lexer.MatchDelim(','))
        {
            _lexer.EatDelim(',');
            list.AddRange(ConstList());
        }
        return list;
    }

    private List<string> FieldList()
    {
        List<string> list = [];
        list.Add(Field);
        if (_lexer.MatchDelim(','))
        {
            _lexer.EatDelim(',');
            list.AddRange(FieldList());
        }
        return list;
    }
}

internal class Insert(string table, List<string> fields, List<Constant> values)
{
    private readonly string _table = table;
    private readonly List<string> _fields = fields;
    private readonly List<Constant> _values = values;
}

internal class Delete(string table, Predicate predicate)
{
    private readonly string _table = table;
    private readonly Predicate _predicate = predicate;
}
