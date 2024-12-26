using SimpleDB.Sql;

namespace SimpleDB.Query;

public interface IScan
{
    public void BeforeFirst();

    public bool Next();

    public int GetInt(string fldName);

    public string GetString(string fldName);

    public Constant GetValue(string fldName);

    public bool HasField(string fldName);

    public void Close();
}
