namespace Common;

public static class Bytes
{
    /// <summary>
    /// Java Integer.BYTES
    /// </summary>
    public static int Integer
    {
        get => BitConverter.GetBytes(int.MaxValue).Length;
    }

    public static int Long
    {
        get => BitConverter.GetBytes(long.MaxValue).Length;
    }
}
