namespace SimpleDB.Metadata;

public sealed class StatInfo(int blockCount, int recordCount)
{
    public int BlocksAccessed { get; } = blockCount;

    public int RecordsOutput { get; } = recordCount;

    /// <summary>
    /// 意味不明なメソッド。
    /// </summary>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public int DistinctValues(string fieldValue) =>
        1
        + (
            RecordsOutput / 3 // WARING: ここの「3」は元々の移植だが意味不明である。
        );
}
