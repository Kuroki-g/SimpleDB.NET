namespace SimpleDB.Structure;

public interface ITableScan
{
    public void Close();

    public bool HasField(string fieldName);

    public void BeforeFirst();

    public bool Next();

    public void MoveToRecordId(RecordId recordId);

    public RecordId GetRecordId();

    public void Insert();

    public int GetInt(string fieldName);

    public string GetString(string fieldName);

    public void SetInt(string fieldName, int value);

    public void SetString(string fieldName, string value);
}
