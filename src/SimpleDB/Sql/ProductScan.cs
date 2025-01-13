using SimpleDB.SqlParser.Grammar;

namespace SimpleDB.Sql;

public class ProductScan : IScan
{
    private readonly IScan _s1;

    private readonly IScan _s2;

    public ProductScan(IScan s1, IScan s2)
    {
        _s1 = s1;
        _s2 = s2;
        BeforeFirst();
    }

    public void BeforeFirst()
    {
        _s1.BeforeFirst();
        _s1.Next();
        _s2.BeforeFirst();
    }

    public void Close()
    {
        _s1.Close();
        _s2.Close();
    }

    public int GetInt(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.GetInt(fieldName) : _s2.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.GetString(fieldName) : _s2.GetString(fieldName);
    }

    public Constant GetValue(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.GetValue(fieldName) : _s2.GetValue(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _s1.HasField(fieldName) || _s2.HasField(fieldName);
    }

    public bool Next()
    {
        if (_s2.Next())
        {
            return true;
        }

        _s2.BeforeFirst();
        // TODO: Javaの書き方とC#の書き方で違いがあるか確認する。
        // 短絡評価がどうなっているか確認する。
        return _s2.Next() && _s1.Next();
    }
}
