namespace SimpleDB.Structure;

public class FieldInfo(int type, int length)
{
    public readonly int Type = type;

    public readonly int Length = length;
}
