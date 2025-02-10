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
    public void Initialize_new_database_and_execute_insert_query()
    {
        // create another directory in order to avoid conflict
        var randomStr = Helper.RandomString(12);
        var dbDir = $"./mock/{randomStr}";

        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        var fm = new FileManager(dbDir, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        // Act
        var db = new Database(dbConfig, fm, lm, bm);
        using var tx = db.NewTx();
        var schema = CreateSchema();
        var layout = new Layout(schema);
        var ts = new TableScan(tx, "tbl", layout);
        var insertedRecords = new List<(int, string)>();
        for (int i = 0; i < 5; i++)
        {
            ts.Insert();
            int n = (int)Math.Round((double)_random.Next() * 50);
            ts.SetInt("A", n);
            ts.SetString("B", "rec" + n);
            insertedRecords.Add((n, "rec" + n));
        }
        tx.Commit();
        tx.Dispose();

        // Assert
        using var tx2 = db.NewTx();
        var ts2 = new TableScan(tx2, "tbl", layout);
        var records = new List<(int, string)>();
        while (ts2.Next())
        {
            records.Add((ts2.GetInt("A"), ts2.GetString("B")));
        }

        Assert.Equal(insertedRecords, records);
    }

    [Fact]
    public void Initialize_created_database()
    {
        // create another directory in order to avoid conflict
        var randomStr = Helper.RandomString(12);
        var dbDir = $"./mock/{randomStr}";

        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        InitializeAndStoreRecords(dbDir, dbConfig);

        // Assert
        using var fm = new FileManager(dbDir, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        // Act
        var db = new Database(dbConfig, fm, lm, bm);
        using var tx2 = db.NewTx();
        var schema = CreateSchema();
        var layout = new Layout(schema);
        var ts2 = new TableScan(tx2, "tbl", layout);
        var records = new List<(int, string)>();
        while (ts2.Next())
        {
            records.Add((ts2.GetInt("A"), ts2.GetString("B")));
        }

        // Assert.Equal(insertedRecords, records);
    }

    private void InitializeAndStoreRecords(string dbDir, SimpleDbConfig dbConfig)
    {
        using var fm = new FileManager(dbDir, dbConfig.BlockSize);
        var lm = new LogManager(fm, dbConfig.LogFileName);
        var bm = new BufferManager(fm, lm, dbConfig.BufferSize);

        // Act
        var db = new Database(dbConfig, fm, lm, bm);
        using var tx = db.NewTx();
        var schema = CreateSchema();
        var layout = new Layout(schema);
        var ts = new TableScan(tx, "tbl", layout);
        var insertedRecords = new List<(int, string)>();
        for (int i = 0; i < 5; i++)
        {
            ts.Insert();
            int n = (int)Math.Round((double)_random.Next() * 50);
            ts.SetInt("A", n);
            ts.SetString("B", "rec" + n);
            insertedRecords.Add((n, "rec" + n));
        }
        tx.Commit();
        tx.Dispose();

        fm.Dispose();
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
    public void Initialize_database_can_open_not_empty_database()
    {
        var dbDir = Path.Combine(_testProjectDir, "System/TestData/9HGMYKROB0DO");
        var randomStr = Helper.RandomString(12);
        var dbDirForTest = Directory.CreateDirectory(Path.Combine(_dir, randomStr));

        foreach (var file in Directory.GetFiles(dbDir))
        {
            var destFile = Path.Combine(_dir, Path.GetFileName(file));
            File.Copy(file, destFile);
        }

        // Arrange
        var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
        using var fm = new FileManager(dbDirForTest.FullName, dbConfig.BlockSize);
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

    [Fact]
    public void CreateTable_can_create_new_table()
    {
        var db = new Database(new SimpleDbConfig());
        using var tx = db.NewTx();
        var schema = CreateSchema();

        // Act
        db.Mm.CreateTable("sample_table", schema, tx);
        tx.Commit();

        // Assert
        var table = db.Mm.GetLayout("sample_table", tx);
        Assert.NotNull(table);
    }

    [Fact]
    public void Recover_can_recover_uncommitted_table_schema()
    {
        var db = new Database(new SimpleDbConfig());
        using var tx = db.NewTx();
        var schema = CreateSchema();

        // Act
        db.Mm.CreateTable("sample_table", schema, tx);
        // recover without commit
        tx.Recover();

        // Assert
        using var tx2 = db.NewTx();
        var table = db.Mm.GetLayout("sample_table", tx2);
        Assert.Null(table);
    }
}
