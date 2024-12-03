using System.Collections;
using System.Runtime.CompilerServices;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

public interface ILogManager : IEnumerable<byte[]>
{
    public int Append(byte[] record);

    public void Flush(int lsn);
}

internal sealed class LogManager : ILogManager
{
    private readonly IFileManager _fm;
    private readonly string _logFile;
    private readonly Page _logPage;
    private BlockId _currentBlock;
    private int _latestLSN = 0;
    private int _lastSavedLSN = 0;

    public LogManager(FileManager fm, string logFile)
    {
        _fm = fm;
        _logFile = logFile;
        byte[] bytes = new byte[fm.BlockSize];
        _logPage = new Page(bytes);

        int logsize = fm.Length(logFile);
        if (logsize == 0)
        {
            _currentBlock = AppendNewBlock();
        }
        else
        {
            _currentBlock = new BlockId(logFile, logsize - 1);
            fm.Read(_currentBlock, _logPage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logRecord"></param>
    /// <returns>ログ シーケンス番号 (LSN) </returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int Append(byte[] logRecord)
    {
        int boundary = _logPage.GetInt(0);
        int recordSize = logRecord.Length;
        // TODO: int bytesNeeded = recordSize + Integer.BYTES;が意味不
        int bytesNeeded = recordSize + 2;
        // It doesn't fit
        if (boundary - bytesNeeded < 2)
        {
            Flush(); // so move to the next block.
            _currentBlock = AppendNewBlock();
            boundary = _logPage.GetInt(0);
        }
        int recpos = boundary - bytesNeeded;
        _logPage.SetBytes(recpos, logRecord);
        _logPage.SetInt(0, recpos); // the new boundary
        _latestLSN += 1;
        return _latestLSN;
    }

    private BlockId AppendNewBlock()
    {
        BlockId block = _fm.Append(_logFile);
        _logPage.SetInt(0, _fm.BlockSize);
        _fm.Write(block, _logPage);
        return block;
    }

    public void Flush(int lsn)
    {
        if (lsn >= _lastSavedLSN)
        {
            Flush();
        }
    }

    private void Flush()
    {
        _fm.Write(_currentBlock, _logPage);
        _lastSavedLSN = _latestLSN;
    }

    public IEnumerator GetEnumerator()
    {
        Flush();
        return new LogIterator(_fm, _currentBlock);
    }

    IEnumerator<byte[]> IEnumerable<byte[]>.GetEnumerator()
    {
        Flush();
        return new LogIterator(_fm, _currentBlock);
    }
}