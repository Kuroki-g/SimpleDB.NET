namespace SimpleDB.System;

public interface ISimpleDbConfig
{
    public int BlockSize { get; }
    public int BufferSize { get; }
    public string FileName { get; }

    public string LogFileName { get; }
}

public class SimpleDbConfig(
    int blockSize = 4096,
    int bufferSize = 8,
    string fileName = "simpledb.db"
) : ISimpleDbConfig
{
    public int BlockSize { get; } = blockSize;

    public int BufferSize { get; } = bufferSize;

    public string FileName { get; } = fileName;

    public string LogFileName => $"{FileName}.log";
}
