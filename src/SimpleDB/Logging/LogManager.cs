using System.Collections;
using System.Runtime.CompilerServices;
using Common;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

public interface ILogManager : IEnumerable<byte[]>
{
    /// <summary>
    /// 与えたバイト列をページに書き込む。
    /// ページのサイズが不足する場合には新しくブロックを追加し、それに書き込む。
    /// </summary>
    /// <param name="logRecord"></param>
    /// <returns>ログ シーケンス番号 (LSN) </returns>
    public int Append(byte[] record);

    public void Flush(int lsn);
}

internal sealed class LogManager : ILogManager
{
    private readonly IFileManager _fm;
    private readonly string _logFile;
    private readonly Page _logPage;
    private BlockId _currentBlock;

    internal BlockId CurrentBlock => _currentBlock;

    private int _latestLsn = 0;

    private int _lastSavedLsn = 0;

    /// <summary>
    /// もしログファイルが存在しない場合には新しくログファイルを設定する。
    /// </summary>
    /// <param name="fm"></param>
    /// <param name="logFile"></param>
    public LogManager(IFileManager fm, string logFile)
    {
        _fm = fm;
        _logFile = logFile;
        var bytes = new byte[fm.BlockSize];
        _logPage = new Page(bytes);

        int logSize = fm.Length(logFile);
        if (logSize == 0)
        {
            _currentBlock = AppendNewBlock();
        }
        else
        {
            _currentBlock = new BlockId(logFile, logSize - 1);
            fm.Read(_currentBlock, _logPage);
        }
    }

    /// <summary>
    /// 与えたバイト列をページに書き込む。
    /// ページのサイズが不足する場合には新しくブロックを追加し、それに書き込む。
    /// </summary>
    /// <param name="logRecord"></param>
    /// <returns>ログ シーケンス番号 (LSN) </returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int Append(byte[] logRecord)
    {
        int boundary = _logPage.GetInt(0);
        int recordSize = logRecord.Length;
        int bytesNeeded = recordSize + Bytes.Integer;
        if (boundary - bytesNeeded < Bytes.Integer) // 必要なサイズに満たない場合
        {
            Flush(); // 次のブロックに移動する。
            _currentBlock = AppendNewBlock();
            boundary = _logPage.GetInt(0);
        }

        int recPos = boundary - bytesNeeded;

        _logPage.SetBytes(recPos, logRecord);
        _logPage.SetInt(0, recPos); // the new boundary

        _latestLsn += 1; // ログ シーケンス番号に1追加する。

        return _latestLsn;
    }

    /// <summary>
    /// 新しいブロックを追加する。
    /// </summary>
    /// <returns></returns>
    private BlockId AppendNewBlock()
    {
        BlockId block = _fm.Append(_logFile);
        _logPage.SetInt(0, _fm.BlockSize);
        _fm.Write(block, _logPage);

        return block;
    }

    public void Flush(int lsn)
    {
        if (lsn >= _lastSavedLsn)
        {
            Flush();
        }
    }

    private void Flush()
    {
        _fm.Write(_currentBlock, _logPage);
        _lastSavedLsn = _latestLsn;
    }

    public IEnumerator<byte[]> GetEnumerator()
    {
        Flush();
        return new LogEnumerator(_fm, _currentBlock);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
