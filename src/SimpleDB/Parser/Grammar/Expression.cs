using SimpleDB.Query;
using SimpleDB.Structure;

namespace SimpleDB.Parser.Grammar;

public class Expression
{
    private readonly Constant? _value = null;

    private readonly string? _fieldName = null;

    public Expression(Constant constant)
    {
        _value = constant;
    }

    public Expression(string fieldName)
    {
        _fieldName = fieldName;
    }

    public Constant Evaluate(IScan scan) => _value is null ? scan.GetValue(_fieldName!) : _value;

    public bool IsFieldName => _fieldName is not null;

    public Constant? AsConstant => _value;

    public string? AsFieldName => _fieldName;

    public bool AppliesTo(ISchema schema) => _value is null ? schema.HasField(_fieldName!) : true;

    public override string ToString() =>
        (_value is null ? _fieldName : _value.ToString()) ?? string.Empty;
}
