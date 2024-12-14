namespace SimpleDB.Structure;

public interface ILayout
{
    ISchema Schema { get; }

    int Offset(string fieldName);

    int SlotSize { get; }
}
