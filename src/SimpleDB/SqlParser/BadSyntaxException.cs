namespace SimpleDB.SqlParser;

public class BadSyntaxException : Exception
{
    public BadSyntaxException()
        : base() { }

    public BadSyntaxException(string message)
        : base(message) { }
}
