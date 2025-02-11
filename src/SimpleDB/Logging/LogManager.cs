using System.Collections;
using System.Runtime.CompilerServices;
using Common;
using SimpleDB.Storage;

namespace SimpleDB.Logging;

public interface ILogManager : IEnumerable<byte[]>, IDisposable
{
    /// <summary>
    /// 与えたバイト列をページに書き込む。
    /// ページのサイズが不足する場合には新しくブロックを追加し、それに書き込む。
    /// </summary>
    /// <param name="record">書き込むバイト列</param>
    /// <returns>ログ シーケンス番号 (LSN) </returns>
    public int Append(byte[] record);

    public void Flush(int lsn);
}

internal sealed class LogManager : ILogManager
{
    private readonly IFileManager _fm;

    private readonly string _logFile;

    private readonly Page _logPage;

    private bool _disposed = false; // Track whether Dispose has been called.

    internal BlockId CurrentBlock { get; private set; }

    private int _latestLsn = 0;

    private int _lastSavedLsn = 0;

    private static LogManager? s_instance = null;

    private static readonly object Lock = new();

    /// <summary>
    /// Singleton instance of the LogManager.
    /// </summary>
    public static LogManager GetInstance(IFileManager fm, string logFile)
    {
        // Double-checked locking for thread safety.
        if (s_instance == null)
        {
            lock (Lock)
            {
                s_instance ??= new LogManager(fm, logFile);
            }
        }
        else //Check if the existing instance uses the same file manager and log file.
        {
            if (s_instance._fm != fm || s_instance._logFile != logFile)
            {
                throw new InvalidOperationException(
                    "LogManager instance already exists with different parameters."
                );
            }
        }

        return s_instance;
    }

    /// <summary>
    /// もしログファイルが存在しない場合には新しくログファイルを設定する。
    /// </summary>
    /// <param name="fm"></param>
    /// <param name="logFile"></param>
    private LogManager(IFileManager fm, string logFile)
    {
        _fm = fm;
        _logFile = logFile;
        var bytes = new byte[fm.BlockSize];
        _logPage = new Page(bytes);

        int logSize = fm.Length(logFile);
        if (logSize == 0)
        {
            CurrentBlock = AppendNewBlock();
        }
        else
        {
            CurrentBlock = new BlockId(logFile, logSize - 1);
            fm.Read(CurrentBlock, _logPage);
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
        ThrowIfDisposed();

        int boundary = _logPage.GetInt(0);
        int recordSize = logRecord.Length;
        int bytesNeeded = recordSize + Bytes.Integer;
        if (boundary - bytesNeeded < Bytes.Integer)
        {
            Flush(); // 次のブロックに移動する。
            CurrentBlock = AppendNewBlock();
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
        ThrowIfDisposed();

        BlockId block = _fm.Append(_logFile);
        _logPage.SetInt(0, _fm.BlockSize);
        _fm.Write(block, _logPage);

        return block;
    }

    public void Flush(int lsn)
    {
        ThrowIfDisposed();

        if (lsn >= _lastSavedLsn)
        {
            Flush();
        }
    }

    private void Flush()
    {
        ThrowIfDisposed();

        _fm.Write(CurrentBlock, _logPage);
        _lastSavedLsn = _latestLsn;
    }

    public IEnumerator<byte[]> GetEnumerator()
    {
        ThrowIfDisposed();

        Flush();
        return new LogEnumerator(_fm, CurrentBlock);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _logPage.Dispose();
            // fm
            if (_fm is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Flush();

            // Important: Clear the static instance reference.
            s_instance = null;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    ~LogManager()
    {
        Dispose(false);
    }
}
