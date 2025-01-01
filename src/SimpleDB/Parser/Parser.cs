namespace SimpleDB.Parser;

public class Parser(string s)
{
    private readonly Lexer _lexer = new(s);

    public string Field => _lexer.EatId();

    public Constant Constant =>
        _lexer.MatchStringConstant()
            ? new Constant(_lexer.EatStringConstant())
            : new Constant(_lexer.EatIntConstant());

    // public Expression Expression =>
    //     _lexer.IsIdMatch ? new Expression(Field) : new Expression(Constant);
}
