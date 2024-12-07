using System.Collections.ObjectModel;
using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.Test.Logging;

internal class LoggingTestHelper
{
    internal static ReadOnlyCollection<(string, int)> GetLogRecords(LogManager lm)
    {
        List<(string, int)> result = [];
        foreach (var record in lm)
        {
            var p = new Page(record);
            var s = p.GetString(0);
            var nPos = Page.MaxLength(s.Length);
            var value = p.GetInt(nPos);

            var message = $"[{s}, {value}]";
            result.Add((s, value));
            Console.WriteLine(message);
        }

        return result.AsReadOnly();
    }

    internal static ReadOnlyCollection<int> CreateSampleLogRecords(
        LogManager lm,
        int start,
        int end
    )
    {
        List<int> result = [];
        for (int i = start; i <= end; i++)
        {
            byte[] record = CreateLogRecord($"record{i}", i + 100);
            int lsn = lm.Append(record);

            result.Add(lsn);
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Create a log record having two values: a string and an integer.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static byte[] CreateLogRecord(string s, int n)
    {
        int sPos = 0;
        int nPos = sPos + Page.MaxLength(s.Length);
        var bytes = new byte[nPos + Bytes.Integer];
        var page = new Page(bytes);
        page.SetString(sPos, s);
        page.SetInt(nPos, n);
        return bytes;
    }
}
