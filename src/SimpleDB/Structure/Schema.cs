using System.Collections.ObjectModel;
using SimpleDB.Sql;

namespace SimpleDB.Structure;

/// <summary>
/// テーブルにおけるレコードのスキーマである。
/// 名前、型を持つ。
/// TODO: 密結合でも問題ないとわかったらISchemaを削除する。
/// </summary>
internal class Schema : ISchema
{
    /// <summary>
    /// NOTE: 暫定的にpublic Listにしているが、書き換えられるといけないことを考慮する。
    /// </summary>
    public ReadOnlyCollection<string> Fields => _fields.AsReadOnly();

    private readonly List<string> _fields = [];

    private readonly Dictionary<string, FieldInfo> _info = [];

    public void Add(string fieldName, ISchema schema)
    {
        AddField(fieldName, schema.Type(fieldName), schema.Length(fieldName));
    }

    public void AddAll(ISchema schema)
    {
        foreach (var fieldName in schema.Fields)
        {
            Add(fieldName, schema);
        }
    }

    public void AddField(string fieldName, int type, int length)
    {
        _fields.Add(fieldName);
        _info[fieldName] = new FieldInfo(type, length);
    }

    public void AddIntField(string fieldName)
    {
        AddField(fieldName, Types.INTEGER, 0);
    }

    public void AddStringField(string fieldName, int length)
    {
        AddField(fieldName, Types.VARCHAR, length);
    }

    public bool HasField(string fieldName) => Fields.Contains(fieldName);

    private FieldInfo GetFieldInfo(string fieldName) =>
        _info.GetValueOrDefault(fieldName) ?? throw new InvalidDataException("not exist field");

    public int Length(string fieldName) => GetFieldInfo(fieldName).Length;

    public int Type(string fieldName) => GetFieldInfo(fieldName).Type;
}
