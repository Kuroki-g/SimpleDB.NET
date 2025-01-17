using SimpleDB.SqlParser.Grammar;
using SimpleDB.SqlParser.Grammar.UpdateCmd;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public interface IUpdatePlanner
{
    // public int ExecuteModify(Update cmd, ITransaction tx);

    public int ExecuteInsert(Insert cmd, ITransaction tx);

    public int ExecuteDelete(Delete cmd, ITransaction tx);

    public int ExecuteCreateTable(CreateTable cmd, ITransaction tx);

    // public int ExecuteCreateView(CreateView cmd, ITransaction tx);

    // public int ExecuteCreateIndex(CreateIndex cmd, ITransaction tx);
}
