namespace TestHelper.Utils;

public static class Helper
{
    private static readonly Random Random = new();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(
            Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray()
        );
    }

    /// <summary>
    /// Clean up specific file of target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void CleanUp(string dbDirectory, string fileName)
    {
        // Clean up
        var info = new FileInfo(Path.Combine(dbDirectory, fileName));
        if (info.Exists)
            info.Delete();
    }

    /// <summary>
    /// Create or clean up target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void InitializeDir(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
            return;
        }

        // Clean up
        foreach (var info in directoryInfo.GetFiles())
        {
            info.Delete();
        }
    }

    /// <summary>
    /// Create or clean up target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void InitializeDir(string dbDirectory) =>
        InitializeDir(new DirectoryInfo(dbDirectory));

    /// <summary>
    /// Clean up target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void CleanUp(string dbDirectory)
    {
        // Clean up
        var dir = new DirectoryInfo(dbDirectory);
        if (!dir.Exists)
            return;

        foreach (var info in dir.GetFiles())
        {
            info.Delete();
        }
    }

    /// <summary>
    /// Delete target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void Delete(string dbDirectory)
    {
        CleanUp(dbDirectory);

        var dir = new DirectoryInfo(dbDirectory);
        if (dir.Exists)
            dir.Delete(true); // Pass 'true' to delete the directory and its contents
    }
}
