using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Storage;
using SimpleDB.Tx.Recovery.LogRecord;
using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Tx.Recovery;

public class RecoveryManager : IRecoveryManager
{
    private readonly int _txNum;

    private readonly ITransaction _tx;

    private readonly ILogManager _lm;
    private readonly IBufferManager _bm;

    public RecoveryManager(ITransaction tx, int txNum, ILogManager lm, IBufferManager bm)
    {
        _txNum = txNum;
        _tx = tx;
        _lm = lm;
        _bm = bm;

        StartRecord.WriteToLog(_lm, _txNum);
    }

    /// <summary>
    /// コミットレコードをログに書き込む。
    /// ディスクにFlushする。
    /// </summary>
    public void Commit()
    {
        _bm.FlushAll(_txNum);
        int lsn = CommitRecord.WriteToLog(_lm, _txNum);
        _lm.Flush(lsn);
    }

    /// <summary>
    /// 未コミットのトランザクションをログからrecoverする。
    /// quiescent checkpointをログに書き込む。
    /// </summary>
    public void Recover()
    {
        DoRecover();
        _bm.FlushAll(_txNum);

        var lsn = CheckpointRecord.WriteToLog(_lm);
        _lm.Flush(lsn);
    }

    /// <summary>
    /// ロールバックレコードをログに書き込み、ディスクにflushする。
    /// </summary>
    public void Rollback()
    {
        DoRollback();
        _bm.FlushAll(_txNum);
        var lsn = RollbackRecord.WriteToLog(_lm, _txNum);
        _lm.Flush(lsn);
    }

    /// <summary>
    /// SetIntRecordをログに書き込む。
    /// TODO: newValueが中で使われていない。要確認。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="newValue"></param>
    /// <returns><see cref="ILogManager.Append"/>を参照のこと。</returns>
    /// <exception cref="BufferAbortException"></exception>
    public int SetInt(Buffer buffer, int offset, int newValue)
    {
        var oldValue = buffer.Contents.GetInt(offset);
        var blockId = buffer.Block ?? throw new BufferAbortException("buffer was not found");

        return SetIntRecord.WriteToLog(_lm, _txNum, blockId, offset, oldValue);
    }

    /// <summary>
    /// SetStringRecordをログに書き込む。
    /// TODO: newValueが中で使われていない。要確認。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="newValue"></param>
    /// <returns><see cref="ILogManager.Append"/>を参照のこと。</returns>
    /// <exception cref="BufferAbortException"></exception>
    public int SetString(Buffer buffer, int offset, string newValue)
    {
        var oldValue = buffer.Contents.GetString(offset);
        var blockId = buffer.Block ?? throw new BufferAbortException("buffer was not found");

        return SetStringRecord.WriteToLog(_lm, _txNum, blockId, offset, oldValue);
    }

    /// <summary>
    /// 登録されているトランザクションをrollbackする。
    /// </summary>
    private void DoRollback()
    {
        foreach (var record in _lm)
        {
            var logRecord = ILogRecord.Create(record);
            if (logRecord.TxNumber == _txNum)
            {
                if (logRecord.Op == TransactionStatus.START)
                    return;
                logRecord.Undo(_tx);
            }
        }
    }

    private void DoRecover()
    {
        List<int> finishedTxs = [];
        foreach (var record in _lm)
        {
            var logRecord = ILogRecord.Create(record) ?? throw new InvalidDataException();
            if (logRecord.Op == TransactionStatus.CHECKPOINT)
                return;

            if (
                logRecord.Op == TransactionStatus.COMMIT
                || logRecord.Op == TransactionStatus.ROLLBACK
            )
            {
                finishedTxs.Add(logRecord.TxNumber);
            }
            else if (!finishedTxs.Contains(logRecord.TxNumber))
            {
                logRecord.Undo(_tx);
            }
        }
    }
}
