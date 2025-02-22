using System.Collections.ObjectModel;
using Common;
using SimpleDB.Logging;
using SimpleDB.Storage;

namespace TestHelper.Utils;

public class LoggingTestHelper
{
    public struct SampleLogRecord
    {
        public string S { get; set; }
        public int N { get; set; }
    }

    public static ReadOnlyCollection<SampleLogRecord> GetLogRecords(ILogManager lm)
    {
        List<SampleLogRecord> result = [];
        foreach (var record in lm)
        {
            var p = new Page(record);
            var s = p.GetString(0);
            var nPos = Page.MaxLength(s.Length);
            var value = p.GetInt(nPos);

            var message = $"[{s}, {value}]";

            result.Add(new SampleLogRecord { S = s, N = value });
            Console.WriteLine(message);
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// (record35, 135) のような組み合わせで、ログを書き込む。
    /// 書き込んだ値の組み合わせを配列にして返す。
    /// </summary>
    /// <param name="lm"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static ReadOnlyCollection<(SampleLogRecord, int lsn)> CreateSampleLogRecords(
        ILogManager lm,
        int start,
        int end
    )
    {
        List<(SampleLogRecord, int)> result = [];
        for (int i = start; i <= end; i++)
        {
            byte[] record = CreateLogRecord($"record{i}", i + 100);
            int lsn = lm.Append(record);
            result.Add((new SampleLogRecord { S = $"record{i}", N = i + 100 }, lsn));
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
