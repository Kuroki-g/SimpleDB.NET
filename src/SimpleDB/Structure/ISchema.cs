using System.Collections.ObjectModel;

namespace SimpleDB.Structure;

public interface ISchema
{
    public void AddField(string fieldName, int type, int length);

    public void AddIntField(string fieldName);

    public void AddStringField(string fieldName, int length);

    public void Add(string fieldName, ISchema schema);

    public void AddAll(ISchema schema);

    ReadOnlyCollection<string> Fields { get; }

    public bool HasField(string fieldName);

    public int Type(string fieldName);

    public int Length(string fieldName);
}
