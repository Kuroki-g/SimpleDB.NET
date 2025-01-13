using SimpleDB.SqlParser.Grammar;

namespace SimpleDB.Sql;

public class ProjectScan(IScan scan, string[] fields) : IScan
{
    private readonly IScan _scan = scan;

    private readonly string[] _fields = fields;

    public void BeforeFirst()
    {
        _scan.BeforeFirst();
    }

    public void Close()
    {
        _scan.Close();
    }

    public int GetInt(string fieldName)
    {
        return HasField(fieldName)
            ? _scan.GetInt(fieldName)
            : throw new FieldNotFoundException(fieldName);
    }

    public string GetString(string fieldName)
    {
        return HasField(fieldName)
            ? _scan.GetString(fieldName)
            : throw new FieldNotFoundException(fieldName);
    }

    public Constant GetValue(string fieldName)
    {
        return HasField(fieldName)
            ? _scan.GetValue(fieldName)
            : throw new FieldNotFoundException(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _fields.Contains(fieldName);
    }

    public bool Next()
    {
        return _scan.Next();
    }
}
