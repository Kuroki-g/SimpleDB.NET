using SimpleDB.Metadata;
using SimpleDB.Sql;
using SimpleDB.SqlParser.Grammar.Create;
using SimpleDB.SqlParser.Grammar.UpdateCmd;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public class BasicUpdatePlanner(IMetadataManager mm) : IUpdatePlanner
{
    private readonly IMetadataManager _mm = mm;

    // Class implementation goes here
    public int ExecuteCreateTable(CreateTable cmd, ITransaction tx)
    {
        _mm.CreateTable(cmd.TableName, cmd.NewSchema, tx);

        return 0;
    }

    public int ExecuteCreateIndex(CreateIndex cmd, ITransaction tx)
    {
        _mm.CreateIndex(cmd.IndexName, cmd.TableName, cmd.FieldName, tx);

        return 0;
    }

    public int ExecuteCreateView(CreateView cmd, ITransaction tx)
    {
        _mm.CreateView(cmd.ViewName, cmd.ViewDef, tx);

        return 0;
    }

    /// <summary>
    /// Execute a delete command
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="tx"></param>
    /// <returns>the number of affected records</returns>
    public int ExecuteDelete(Delete cmd, ITransaction tx)
    {
        var tablePlan = new TablePlan(tx, cmd.Table, _mm);
        var selectPlan = new SelectPlan(tablePlan, cmd.Predicate);
        var scan = (IUpdateScan)selectPlan.Open();
        var count = 0;
        while (scan.Next())
        {
            scan.Delete();
            count++;
        }
        scan.Close();

        return count;
    }

    /// <summary>
    /// Execute an insert command
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="tx"></param>
    /// <returns>the number of affected records</returns>
    public int ExecuteInsert(Insert cmd, ITransaction tx)
    {
        var plan = new TablePlan(tx, cmd.Table, _mm);
        var scan = (IUpdateScan)plan.Open();
        scan.Insert();
        for (var i = 0; i < cmd.Fields.Count; i++)
        {
            var fld = cmd.Fields[i];
            var value = cmd.Values[i];
            scan.SetValue(fld, value);
        }
        scan.Close();

        return 1;
    }

    public int ExecuteModify(Modify cmd, ITransaction tx)
    {
        var plan = new TablePlan(tx, cmd.TableName, _mm);
        var selectPlan = new SelectPlan(plan, cmd.Predicate);
        var scan = (IUpdateScan)selectPlan.Open();
        var count = 0;
        while (scan.Next())
        {
            var value = cmd.NewValue.Evaluate(scan);
            scan.SetValue(cmd.TargetField, value);
            count++;
        }
        scan.Close();

        return count;
    }
}
