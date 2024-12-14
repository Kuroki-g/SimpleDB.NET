using Common;
using SimpleDB.Sql;
using SimpleDB.Storage;

namespace SimpleDB.Structure;

public class Layout : ILayout
{
    public ISchema Schema { get; }

    public int SlotSize { get; }

    public int Offset(string fieldName) => _offsets.GetValueOrDefault(fieldName);

    private readonly Dictionary<string, int> _offsets = [];

    public Layout(ISchema schema)
    {
        Schema = schema;
        var pos = Bytes.Integer;
        foreach (var fieldName in schema.Fields)
        {
            _offsets[fieldName] = pos;
            pos += LenghInBytes(fieldName);
        }

        SlotSize = pos;
    }

    public Layout(ISchema schema, Dictionary<string, int> offsets, int slotSize)
    {
        Schema = schema;
        _offsets = offsets;
        SlotSize = slotSize;
    }

    private int LenghInBytes(string fieldName)
    {
        var fildType = Schema.Type(fieldName);
        return fildType == Types.INTEGER ? Bytes.Integer : Page.MaxLength(Schema.Length(fieldName));
    }
}
