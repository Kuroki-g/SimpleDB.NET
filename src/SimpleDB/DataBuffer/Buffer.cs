using SimpleDB.Logging;
using SimpleDB.Storage;

namespace SimpleDB.DataBuffer;

public class Buffer : IDisposable
{
    private readonly IFileManager _fm;

    private readonly ILogManager _lm;

    private int _lsn = -1;

    private int _pins = 0;

    private bool _disposed = false; // Dispose済みフラグ

    public Page Contents { get; init; }

    public BlockId? Block { get; private set; } = null;

    public Buffer(IFileManager fileManager, ILogManager logManager)
    {
        _fm = fileManager;
        _lm = logManager;
        Contents = new Page(_fm.BlockSize);
    }

    public bool IsPinned => _pins > 0;

    public int ModifyingTx { get; private set; } = -1;

    public void SetModified(int txNumber, int lsn)
    {
        ModifyingTx = txNumber;
        if (lsn >= 0)
        {
            _lsn = lsn;
        }
    }

    internal void AssignToBlock(BlockId blockId)
    {
        Flush();
        Block = blockId;
        _fm.Read(Block, Contents);
        _pins = 0;
    }

    internal void Flush()
    {
        if (ModifyingTx < 0)
            return;

        _lm.Flush(_lsn);
        if (Block is null)
        {
            throw new InvalidOperationException("Block must be allocated before write contents.");
        }
        _fm.Write(Block, Contents);
        ModifyingTx = -1;
    }

    internal void Pin()
    {
        _pins++;
    }

    internal void Unpin()
    {
        _pins--;
    }

    // Public Disposeメソッド
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected Disposeメソッド (パターン)
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // マネージドリソースの解放
                Contents.Dispose(); // PageオブジェクトをDispose

                if (_fm is IDisposable disposableFm)
                {
                    disposableFm.Dispose();
                }
                if (_lm is IDisposable disposableLm)
                {
                    disposableLm.Dispose();
                }
            }

            _disposed = true;
        }
    }

    ~Buffer()
    {
        Dispose(false);
    }
}
