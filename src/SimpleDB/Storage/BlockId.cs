namespace SimpleDB.Storage;

public sealed class BlockId
{
    public string FileName { get; private set; }
    public int Number { get; private set; }

    public BlockId(string fileName, int blockNumber)
    {
        if (fileName == string.Empty)
            throw new InvalidOperationException($"{fileName} must not be empty.");
        FileName = fileName;
        if (blockNumber < 0)
            throw new InvalidOperationException($"{blockNumber} must not be positive integer.");
        Number = blockNumber;
    }

    public bool Equals(BlockId? another)
    {
        return FileName == another?.FileName && Number == another.Number;
    }

    public override string ToString()
    {
        return $"[file {FileName}, block {Number}]";
    }

    public int HashCode()
    {
        return GetHashCode();
    }
}
