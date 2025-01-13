using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Sql;

public class SelectScan : IUpdateScan
{
    private readonly IUpdateScan _scan;

    private readonly Predicate _predicate;

    public SelectScan(IUpdateScan scan, Predicate predicate)
    {
        _scan = scan;
        _predicate = predicate;
    }

    public void BeforeFirst()
    {
        _scan.BeforeFirst();
    }

    public void Close()
    {
        _scan.Close();
    }

    public void Delete()
    {
        _scan.Delete();
    }

    public int GetInt(string fieldName)
    {
        return _scan.GetInt(fieldName);
    }

    public RecordId GetRecordId()
    {
        return _scan.GetRecordId();
    }

    public string GetString(string fieldName)
    {
        return _scan.GetString(fieldName);
    }

    public Constant GetValue(string fieldName)
    {
        return _scan.GetValue(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _scan.HasField(fieldName);
    }

    public void Insert()
    {
        _scan.Insert();
    }

    public void MoveToRecordId(RecordId rid)
    {
        _scan.MoveToRecordId(rid);
    }

    public bool Next()
    {
        while (_scan.Next())
        {
            if (_predicate.IsSatisfied(_scan))
            {
                return true;
            }
        }

        return false;
    }

    public void SetInt(string fieldName, int val)
    {
        _scan.SetInt(fieldName, val);
    }

    public void SetString(string fieldName, string val)
    {
        _scan.SetString(fieldName, val);
    }

    public void SetValue(string fieldName, Constant val)
    {
        _scan.SetValue(fieldName, val);
    }
}
