using SimpleDB.Storage;

namespace SimpleDB.Test.Storage;

public class PageTest
{
    [Theory]
    [InlineData(0, 0, new byte[] { }, new byte[] { })] // BlockSizeが0の場合に空のバイト列を格納できる
    [InlineData(1, 0, new byte[] { 255 }, new byte[] { 255 })] // BlockSizeが1の場合に指定のバイト列を格納できる
    [InlineData(2, 0, new byte[] { 255, 255 }, new byte[] { 255, 255 })] // BlockSizeが1より大きい場合に指定のバイト列を格納できる
    [InlineData(2, 1, new byte[] { 255 }, new byte[] { 0, 255 })] // BlockSizeが1の場合に指定のバイト列を格納できる
    public void SetBytes_GetBytes_バイト列での値のsetとgetが出来る(
        int blockSize,
        int offset,
        byte[] bytes,
        byte[] expected
    )
    {
        var page = new Page(blockSize);
        page.SetBytes(offset, bytes);

        Assert.Equal(expected, page.GetBytes(0));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)] // BlockSizeが0の場合に0を格納できる
    [InlineData(1, 0, 255, 255)] // BlockSizeが1の場合に指定のint列を格納できる
    [InlineData(10, 0, int.MaxValue, int.MaxValue)] // BlockSizeがint.MaxValueの場合に指定のint列を格納できる
    [InlineData(10, 0, int.MinValue, int.MinValue)] // BlockSizeがint.MinValueの場合に指定のint列を格納できる
    public void SetInt_GetInt_Int32のsetとgetができる(
        int blockSize,
        int offset,
        int intNum,
        int expected
    )
    {
        var page = new Page(blockSize);
        page.SetInt(offset, intNum);

        Assert.Equal(expected, page.GetInt(0));
    }

    [Theory]
    [InlineData(1, 0, "a", "a")] // In case of ASCII
    [InlineData(4, 0, "a", "a\0\0\0")] // In case of ASCII
    [InlineData(4, 0, "あ", "あ\0")] // In case of Japanese character (3 bytes)
    [InlineData(4, 0, "🍺", "🍺")] // In case of emoji
    [InlineData(4, 0, "𩸽", "𩸽")] // In case of 4 bytes string
    public void SetString_GetString_文字列のsetとgetができる(
        int blockSize,
        int offset,
        string str,
        string expected
    )
    {
        var page = new Page(blockSize);
        page.SetString(offset, str);

        Assert.Equal(expected, page.GetString(0));
    }

    [Fact]
    public void Contents_正しく格納されたバイト列が取得できる()
    {
        var page = new Page(8);

        page.SetString(0, "🍺");
        page.SetInt(Page.CHARSET.GetByteCount("🍺"), 255);
        page.SetBytes(Page.CHARSET.GetByteCount("🍺") + 1, [0xF0]);

        Assert.Equal([0xF0, 0x9F, 0x8D, 0xBA, 255, 0xF0, 0, 0], page.Contents());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(10, 40)]
    public void MaxLength(int strlen, int expected)
    {
        var actual = Page.MaxLength(strlen);

        Assert.Equal(expected, actual);
    }
}
