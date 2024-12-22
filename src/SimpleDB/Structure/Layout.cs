using Common;
using SimpleDB.Sql;
using SimpleDB.Storage;

namespace SimpleDB.Structure;

/// <summary>
/// レコードの構造を保有するためのクラスである。
/// </summary>
public class Layout
{
    /// <summary>
    /// テーブルのスキーマを返す。
    /// </summary>
    public ISchema Schema { get; }

    public int SlotSize { get; }

    /// <summary>
    /// 指定のフィールドのoffsetを返す。
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public int Offset(string fieldName) => _offsets.GetValueOrDefault(fieldName);

    private readonly Dictionary<string, int> _offsets = [];

    public Layout(ISchema schema)
    {
        Schema = schema;
        var pos = Bytes.Integer;
        foreach (var fieldName in schema.Fields)
        {
            _offsets[fieldName] = pos;
            pos += LengthInBytes(fieldName);
        }

        SlotSize = pos;
    }

    public Layout(ISchema schema, Dictionary<string, int> offsets, int slotSize)
    {
        Schema = schema;
        _offsets = offsets;
        SlotSize = slotSize;
    }

    private int LengthInBytes(string fieldName)
    {
        var fieldType = Schema.Type(fieldName);
        return fieldType == Types.INTEGER
            ? Bytes.Integer
            : Page.MaxLength(Schema.Length(fieldName));
    }
}
