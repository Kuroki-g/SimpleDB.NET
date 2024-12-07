namespace TestHelper.Utils;

public static class Helper
{
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
    /// Clean up target directory.
    /// </summary>
    /// <param name="dbDirectory"></param>
    /// <param name="fileName"></param>
    public static void CleanUp(string dbDirectory)
    {
        // Clean up
        var dir = new DirectoryInfo(dbDirectory);
        foreach (var info in dir.GetFiles())
        {
            info.Delete();
        }
    }
}
