using Common;
using SimpleDB.Feat.Test.Tx;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Feat.Test.Logging;

public class LogTest : IntegrationTestBase
{
    private static LogManager? s_lm;

    [Fact]
    public void Log_必要な出力がなされる()
    {
        FileManager.GetInstance(
            new FileManagerConfig()
            {
                DbDirectory = _dir,
                FileName = "logtest",
                BlockSize = 200,
            }
        );
        s_lm = LogManager.GetInstance(FileManager.GetInstance(), "logtest");

        // print an empty log file
        var emptyResult = GetLogRecords();
        Assert.Empty(emptyResult);

        CreateRecords(1, 35);
        // "The log file now has these records:"
        GetLogRecords();
        CreateRecords(36, 70);
        s_lm.Flush(65);
        // "The log file now has these records:"
        GetLogRecords();
    }

    private static List<string[]> GetLogRecords()
    {
        Console.WriteLine("The log file now has these records:");
        var results = new List<string[]>();
        var iter = s_lm!.GetEnumerator();
        while (iter.MoveNext())
        {
            var rec = iter.Current;
            using var p = new Page(rec);
            var s = p.GetString(0);
            var npos = Bytes.Integer + Page.CHARSET.GetBytes(s).Length; // 文字列の長さ(int) + 文字列自体のバイト数
            var val = p.GetInt(npos);
            results.Add([s, val.ToString()]);
            Console.WriteLine($"Record: {s}, {val}");
        }

        return results;
    }

    private static void CreateRecords(int start, int end)
    {
        Console.Write("Creating records: ");
        for (int i = start; i <= end; i++)
        {
            var rec = CreateLogRecord($"record{i}", i + 100);
            var lsn = s_lm!.Append(rec);
            Console.Write($"{lsn} ");
        }
        Console.WriteLine();
    }

    // Create a log record having two values: a string and an integer.
    private static byte[] CreateLogRecord(string s, int n)
    {
        int spos = 0;
        int npos = Bytes.Integer + Page.CHARSET.GetBytes(s).Length; // 文字列の長さ(int) + 文字列自体のバイト数
        var b = new byte[npos + Bytes.Integer];
        using var p = new Page(b);
        p.SetString(spos, s);
        p.SetInt(npos, n);
        return b;
    }
}
