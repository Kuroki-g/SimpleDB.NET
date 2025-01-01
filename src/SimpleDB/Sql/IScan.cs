namespace SimpleDB.Sql;

public interface IScan
{
    public void BeforeFirst();

    public bool Next();

    public int GetInt(string fieldName);

    public string GetString(string fieldName);

    public Constant GetValue(string fieldName);

    public bool HasField(string fieldName);

    public void Close();
}
