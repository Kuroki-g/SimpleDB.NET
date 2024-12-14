using SimpleDB.Storage;

namespace SimpleDB.Structure;

public interface IRecordPage
{
    public int GetInt(int slot, string fieldName);

    public string GetString(int slot, string fieldName);

    public void SetInt(int slot, string fieldName, int value);

    public void SetString(int slot, string fieldName, string value);

    public void Format();

    public void Delete(int slot);

    public int NextAfter(int slot);

    public int InsertAfter(int slot);

    public BlockId BlockId { get; }
}
