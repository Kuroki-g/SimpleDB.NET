namespace SimpleDB.Parser.Grammar;

public class Constant : IComparable
{
    private readonly int? _intValue = null;

    private readonly string? _stringValue = null;

    private readonly bool _isInt;

    private readonly bool _isString;

    public Constant(int value)
    {
        _intValue = value;
        _isInt = true;
    }

    public Constant(string value)
    {
        _stringValue = value;
        _isString = true;
    }

    public int? AsInt() => _intValue;

    public string? AsString() => _stringValue;

    public override int GetHashCode()
    {
        return _isInt ? _intValue.GetHashCode()
            : _isString ? _isString.GetHashCode()
            : throw new InvalidDataException();
    }

    public override bool Equals(object? obj)
    {
        var constant = (Constant?)obj;
        return constant is not null
            && (
                _isInt ? _intValue.Equals(constant._intValue)
                : _isString ? _stringValue!.Equals(constant._stringValue)
                : throw new InvalidDataException()
            );
    }

    public int CompareTo(object? obj)
    {
        var constant = (Constant?)obj ?? throw new InvalidDataException();
        return _isInt ? _intValue!.Value.CompareTo(constant._intValue)
            : _isString ? _stringValue!.CompareTo(constant._stringValue)
            : throw new InvalidDataException();
    }
}
