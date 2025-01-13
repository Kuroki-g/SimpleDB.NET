namespace SimpleDB.Sql;

public class FieldNotFoundException : Exception
{
    public FieldNotFoundException() { }

    public FieldNotFoundException(string fieldName)
        : base($"field {fieldName} not found.") { }

    public FieldNotFoundException(string fieldName, Exception inner)
        : base($"field {fieldName} not found.", inner) { }
}
