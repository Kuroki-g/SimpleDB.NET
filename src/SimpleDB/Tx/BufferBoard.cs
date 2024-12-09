using SimpleDB.Storage;
using Buffer = SimpleDB.DataBuffer.Buffer;

namespace SimpleDB.Tx;

/// <summary>
/// Bufferに関するオブジェクトを管理する。
/// </summary>
/// <param name="bm"></param>
internal class BufferBoard(IBufferManager bm)
{
    private readonly Dictionary<BlockId, Buffer> _bufferDict = [];

    private readonly List<BlockId> _pins = [];

    private readonly IBufferManager _bm = bm;

    internal Buffer? GetBuffer(BlockId blockId) => _bufferDict.GetValueOrDefault(blockId);

    internal int BufferCount => _bufferDict.Count;

    internal void Pin(BlockId blockId)
    {
        var buffer = _bm.Pin(blockId);
        _bufferDict[blockId] = buffer;
        _pins.Add(blockId);
    }

    internal void Unpin(BlockId blockId)
    {
        var buffer = _bufferDict.GetValueOrDefault(blockId);
        if (buffer is not null)
            _bm.Unpin(buffer);

        _pins.Remove(blockId);

        if (!_pins.Contains(blockId))
        {
            _bufferDict.Remove(blockId);
        }
    }

    internal void UnpinAll()
    {
        foreach (var blockId in _pins)
        {
            var buffer = _bufferDict.GetValueOrDefault(blockId);
            if (buffer is not null)
                _bm.Unpin(buffer);
        }
        _bufferDict.Clear();
        _pins.Clear();
    }
}
