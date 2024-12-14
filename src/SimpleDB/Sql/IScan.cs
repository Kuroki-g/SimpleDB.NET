namespace SimpleDB.Sql;

public interface IScan
{
    public void BeforeFirst();

    public bool Next();

    public int GetInt();

    public string GetString();

    public bool HasField(string fieldName);

    public void Close();
}
