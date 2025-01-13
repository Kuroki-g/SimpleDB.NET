using SimpleDB.SqlParser.Grammar;
using SimpleDB.Structure;

namespace SimpleDB.Sql;

public interface IUpdateScan : IScan
{
    public void SetInt(string fieldName, int val);

    public void SetString(string fieldName, string val);

    public void SetValue(string fieldName, Constant val);

    public void Insert();

    public void Delete();

    public RecordId GetRecordId();

    public void MoveToRecordId(RecordId rid);
}
