using SimpleDB.Storage;
using SimpleDB.System;

namespace SimpleDB.Feat.Test.Storage;

public class FileTest
{
    [Fact]
    public void Database経由で取得した設定を用いてPageへの書き込みを行うことができる()
    {
        var config = new SimpleDbConfig();
        var db = new Database(config);
        var fm = db.Fm;
        var blk = new BlockId("testfile", 2);
        int pos1 = 88;

        // Act
        var p1 = new Page(fm.BlockSize);
        p1.SetString(pos1, "abcdefghijklm");
        int size = Page.MaxLength("abcdefghijklm".Length);
        int pos2 = pos1 + size;
        p1.SetInt(pos2, 345);
        fm.Write(blk, p1);

        var p2 = new Page(fm.BlockSize);
        fm.Read(blk, p2);

        // Assert
        Assert.Equal("abcdefghijklm", p2.GetString(pos1));
        Assert.Equal(345, p2.GetInt(pos2));
    }
}
