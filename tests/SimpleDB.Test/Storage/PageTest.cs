using SimpleDB.Storage;

namespace SimpleDB.Test.Storage;

public class PageTest
{
    [Fact]
    public void Page_intで初期化できる()
    {
        var page = new Page(2);

        var actual = page.Contents();

        Assert.Equal([0, 0], actual);
    }

    [Fact]
    public void Page_バイト列で初期化できる()
    {
        var page = new Page([0, 0]);

        var actual = page.Contents();

        Assert.Equal([0, 0], actual);
    }

    [Fact]
    public void SetBytes()
    {
        var page = new Page(10);
        byte[] expected = [4, 0, 0, 0, 0x01, 0x02, 0x03, 0x04, 0, 0];
        page.SetBytes(0, [0x01, 0x02, 0x03, 0x04]);

        var actual = page.Contents();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetBytes_コンストラクタで直接バイト列をセットした場合()
    {
        var page = new Page(
            [
                4,
                0,
                0,
                0,
                // 長さを指定するintの後のここから4つ取得する。
                0x01,
                0x02,
                0x03,
                0x04,
                0,
                0,
            ]
        );

        var actual = page.GetBytes(0);

        Assert.Equal([0x01, 0x02, 0x03, 0x04], actual);
    }

    [Theory]
    [InlineData(8, 1, new byte[] { 255 })]
    [InlineData(8, 1, new byte[] { 255, 0, 0 })]
    public void SetBytes_GetBytes_範囲外の場合(int blockSize, int offset, byte[] bytes)
    {
        var page = new Page(blockSize);
        page.SetBytes(offset, bytes);

        var actual = Record.Exception(() => page.GetBytes(0));

        Assert.IsType<ArgumentOutOfRangeException>(actual);
    }

    [Theory]
    [InlineData(8, 0, new byte[] { 255 }, new byte[] { 255 })]
    [InlineData(8, 0, new byte[] { 255, 255 }, new byte[] { 255, 255 })]
    public void SetBytes_GetBytes_バイト列での値のsetとgetが出来る(
        int blockSize,
        int offset,
        byte[] bytes,
        byte[] expected
    )
    {
        var page = new Page(blockSize);
        page.SetBytes(offset, bytes);

        var actual = page.GetBytes(0);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(8, 0, 1, 1)] // BlockSizeが1の場合に指定のint列を格納できる
    [InlineData(8, 0, int.MaxValue, int.MaxValue)] // int.MaxValueの場合に指定のint列を格納できる
    [InlineData(8, 0, int.MinValue, int.MinValue)] // int.MinValueの場合に指定のint列を格納できる
    public void SetInt_GetInt_Int32のsetとgetができる(
        int blockSize,
        int offset,
        int intNum,
        int expected
    )
    {
        var page = new Page(blockSize);
        page.SetInt(offset, intNum);

        var actual = page.GetInt(0);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(8, 0, "a")] // In case of ASCII
    [InlineData(8, 0, "あ")] // In case of Japanese character (3 bytes)
    [InlineData(8, 0, "🍺")] // In case of emoji
    [InlineData(8, 0, "𩸽")] // In case of 4 bytes string
    [InlineData(32, 0, "aあ𩸽")] // In case of 4 bytes string
    public void SetString_GetString_文字列のsetとgetができる(int blockSize, int offset, string str)
    {
        var page = new Page(blockSize);
        page.SetString(offset, str);
        var expected = str;

        var actual = page.GetString(0);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(1, 10)]
    [InlineData(2, 16)]
    [InlineData(3, 22)]
    [InlineData(4, 28)]
    [InlineData(10, 64)]
    public void MaxLength(int strlen, int expected)
    {
        var actual = Page.MaxLength(strlen);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Dispose_IsCalled_WithUsingStatement()
    {
        // Arrange (準備)
        var blockSize = 4096;
        Page page;

        // Act (実行)
        using (page = new Page(blockSize))
        {
            // 何か Page を使う処理 (ここでは省略)
        }

        // Assert (検証)
        Assert.True(page.IsDisposed, "Dispose() was not called.");
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var blockSize = 4096;
        var page = new Page(blockSize);

        // Act
        page.Dispose();
        var actual = Record.Exception(() => page.Dispose());

        // Assert
        Assert.True(page.IsDisposed, "Dispose() was not called.");
        // 例外が発生しないことを確認
        Assert.Null(actual);
    }
}
