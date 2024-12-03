using SimpleDB.Storage;

namespace SimpleDB.Test.Storage;

public class BlockIdTest
{
    [Fact]
    public void ToString_Can_obtain_block_description()
    {
        var blockID = new BlockId("file_name", 1);
        var expected = "[file file_name, block 1]";

        var actual = blockID.ToString();

        Assert.Equal(expected, actual);
    }

    class EqualTestData : TheoryData<(string, int), (string, int), bool>
    {
        public EqualTestData()
        {
            Add(("file_name", 1), ("file_name", 1), true); // 同じfileNameとblockNumberの場合
            Add(("file_name", 1), ("another_file_name", 1), false); // 同じfileNameとblockNumberの場合
            Add(("file_name", 1), ("file_name", 2), false); // 同じfileNameとblockNumberの場合
        }
    }

    [Theory]
    [ClassData(typeof(EqualTestData))]
    public void Equal_Distinguish_file_name_and_block_number(
        (string, int) one,
        (string, int) another,
        bool expected
    )
    {
        var blockId = new BlockId(one.Item1, one.Item2);
        var anotherBlockId = new BlockId(another.Item1, another.Item2);

        var acutual = blockId.Equals(anotherBlockId);

        Assert.Equal(expected, acutual);
    }
}
