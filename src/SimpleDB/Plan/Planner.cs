using SimpleDB.SqlParser;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.SqlParser.Grammar.Create;
using SimpleDB.SqlParser.Grammar.UpdateCmd;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public class Planner(IQueryPlanner queryPlanner, IUpdatePlanner commandPlanner)
{
    private readonly IQueryPlanner _queryPlanner = queryPlanner;

    private readonly IUpdatePlanner _commandPlanner = commandPlanner;

    public IPlan CreatePlan(string query, ITransaction tx)
    {
        var parser = new Parser(query);
        var cmd = parser.Query();
        VerifyQuery(cmd);
        return _queryPlanner.CreatePlan(cmd, tx);
    }

    private void VerifyQuery(Query cmd)
    {
        // TODO: throw new NotImplementedException();
    }

    /// <summary>
    /// UpdateCmd term
    /// </summary>
    /// <returns></returns>
    public int ExecuteUpdateCmd(string cmd, ITransaction tx)
    {
        var parser = new Parser(cmd);
        var updateCmd = parser.UpdateCmd();
        VerifyUpdateCmd(updateCmd);
        // check updateCmd instance and return _commandPlanner.ExecuteUpdateCmd(updateCmd, tx);
        return updateCmd switch
        {
            Insert insert => _commandPlanner.ExecuteInsert(insert, tx),
            Delete delete => _commandPlanner.ExecuteDelete(delete, tx),
            // TODO: Implement Modify
            CreateTable createTable => _commandPlanner.ExecuteCreateTable(createTable, tx),
            // TODO: Implement CreateView, CreateIndex
            _ => default,
        };
    }

    private void VerifyUpdateCmd(object updateCmd)
    {
        // TODO: throw new NotImplementedException();
    }
}
