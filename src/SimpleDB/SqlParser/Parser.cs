using System.Collections.ObjectModel;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.SqlParser.Grammar.UpdateCmd;
using SimpleDB.Structure;

namespace SimpleDB.SqlParser;

public class Parser(string s)
{
    private readonly Lexer _lexer = new(s);

    public string Field()
    {
        return _lexer.EatIdentifier();
    }

    public Constant Constant =>
        _lexer.MatchStringConstant()
            ? new Constant(_lexer.EatStringConstant())
            : new Constant(_lexer.EatIntConstant());

    public Expression Expression =>
        _lexer.MatchIdentifier() ? new Expression(Field()) : new Expression(Constant);

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
        fields.Add(Field());
        while (_lexer.MatchDelimiter(','))
        {
            _lexer.EatDelim(',');
            fields.AddRange(SelectList());
        }
        return fields;
    }

    private IEnumerable<string> TableList()
    {
        List<string> tables = [];
        tables.Add(_lexer.EatIdentifier());
        while (_lexer.MatchDelimiter(','))
        {
            _lexer.EatDelim(',');
            tables.AddRange(TableList());
        }
        return tables;
    }

    public IUpdateCmd UpdateCmd()
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

    private IUpdateCmd Create()
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

    private ICreate CreateIndex()
    {
        throw new NotImplementedException();
    }

    private ICreate CreateView()
    {
        throw new NotImplementedException();
    }

    private CreateTable CreateTable()
    {
        _lexer.EatKeyword("table");
        var tableName = _lexer.EatIdentifier();
        _lexer.EatDelim('(');
        var schema = FieldDefs();
        _lexer.EatDelim(')');

        return new CreateTable(schema, tableName);
    }

    private Schema FieldDefs()
    {
        var schema = FieldDef();
        if (_lexer.MatchDelimiter(','))
        {
            _lexer.EatDelim(',');
            schema.AddAll(FieldDefs());
        }
        return schema;
    }

    private Schema FieldDef()
    {
        var fieldName = Field();
        return FieldType(fieldName);
    }

    private Schema FieldType(string fieldName)
    {
        var schema = new Schema();

        if (_lexer.MatchKeyword("int"))
        {
            _lexer.EatKeyword("int");
            schema.AddIntField(fieldName);
        }
        else
        {
            // parse varchar(n)
            _lexer.EatKeyword("varchar");
            _lexer.EatDelim('(');
            var n = _lexer.EatIntConstant();
            _lexer.EatDelim(')');
            schema.AddStringField(fieldName, n);
        }
        return schema;
    }

    private IUpdateCmd Modify()
    {
        throw new NotImplementedException();
    }

    private Delete Delete()
    {
        _lexer.EatKeyword("delete");
        _lexer.EatKeyword("from");
        var table = _lexer.EatIdentifier();
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
        var table = _lexer.EatIdentifier();
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
        if (_lexer.MatchDelimiter(','))
        {
            _lexer.EatDelim(',');
            list.AddRange(ConstList());
        }
        return list;
    }

    private List<string> FieldList()
    {
        List<string> list = [];
        list.Add(Field());
        if (_lexer.MatchDelimiter(','))
        {
            _lexer.EatDelim(',');
            list.AddRange(FieldList());
        }
        return list;
    }
}
