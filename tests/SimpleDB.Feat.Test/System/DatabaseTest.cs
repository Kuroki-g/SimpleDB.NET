using SimpleDB.DataBuffer;
using SimpleDB.Feat.Test.Tx;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Structure;
using SimpleDB.System;
using TestHelper.Utils;

namespace SimpleDB.Feat.Test.System;

public class DatabaseTest : IntegrationTestBase
{
    [Fact]
    public void Initialize_new_database_and_execute_some_query()
    {
        // create another directory in order to avoid conflict
        var randomStr = Helper.RandomString(12);
        var dbDir = $"./mock/{randomStr}";

        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        var fm = new FileManager(dbDir, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        var db = new Database(dbConfig, fm, lm, bm);
        using var tx = db.NewTx();
        var schema = CreateSchema();
        var layout = new Layout(schema);
        var us = new TableScan(tx, "tbl", layout);

        // Act

        // Assert
        Assert.Null(result);
    }
}
