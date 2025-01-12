using SimpleDB.Storage;

namespace SimpleDB.System;

public sealed class Database
{
    public readonly int BlockSize;

    public Database(ISimpleDbConfig dbConfig)
    {
        BlockSize = dbConfig.BlockSize;

        var fm = new FileManager(dbConfig.FileName, BlockSize);
    }

    // public FileManager FileManager;
}
