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
        // Assert.Null(result);
    }

    [Fact]
    public void Initialize_new_database_can_commit_empty_transaction()
    {
        // create another directory in order to avoid conflict
        var randomStr = Helper.RandomString(12);
        var dbDir = $"./mock/{randomStr}";

        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        var fm = new FileManager(dbDir, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        var fn = new Action(() =>
        {
            var db = new Database(dbConfig, fm, lm, bm);
            using var tx = db.NewTx();
            tx.Commit();
            tx.Dispose();
        });

        // Act
        var result = Record.Exception(fn);

        Assert.Null(result);
    }

    [Fact]
    public void Initialize_database_can_open_empty_database()
    {
        var dbDir = Path.Combine(_testProjectDir, "System/TestData/simple.db");
        var randomStr = Helper.RandomString(12);
        var dbDirForTest = Directory.CreateDirectory(Path.Combine(_dir, randomStr));

        foreach (var file in Directory.GetFiles(dbDir))
        {
            var destFile = Path.Combine(_dir, Path.GetFileName(file));
            File.Copy(file, destFile);
        }

        // create another directory in order to avoid conflict
        // dbdir is tests/SimpleDB.Feat.Test/System/TestData/simple.db


        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        var fm = new FileManager(dbDirForTest.FullName, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        var fn = new Action(() =>
        {
            var db = new Database(dbConfig, fm, lm, bm);
            using var tx = db.NewTx();
            tx.Commit();
            tx.Dispose();
        });

        // Act
        var result = Record.Exception(fn);

        Assert.Null(result);
    }
}
