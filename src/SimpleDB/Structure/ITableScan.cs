using SimpleDB.Sql;

namespace SimpleDB.Structure;

public interface ITableScan : IScan
{
    public void MoveToRecordId(RecordId recordId);

    public RecordId GetRecordId();

    public void Insert();

    public void Delete();

    public void SetInt(string fieldName, int value);

    public void SetString(string fieldName, string value);
}
