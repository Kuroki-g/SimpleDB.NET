namespace SimpleDB.Structure;

public class RecordId(int blockNumber, int slot)
{
    public readonly int BlockNumber = blockNumber;
    public readonly int Slot = slot;

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        var r = (RecordId)obj;
        return BlockNumber == r.BlockNumber && Slot == r.Slot;
    }

    public override string ToString() => $"[{BlockNumber}, {Slot}]";
}
