using Grpc.Core;
using SimDbGrpc;
using SimpleDB.System;

namespace SimpleDB.Services;

public class SqlService(Database db, ILogger<SqlService> logger) : SimDbGrpc.Sql.SqlBase
{
    private readonly Database _db = db;
    private readonly ILogger<SqlService> _logger = logger;

    public override Task<SqlResponse> CreateCommand(SqlRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Command: {Command}", request.Command);
        var tx = _db.NewTx();
        // todo: とりあえずqueryのみ実行可能にしている
        var plan = _db.Planner.CreatePlan(request.Command, tx);
        var scan = plan.Open();
        while (scan.Next())
        {
            _logger.LogInformation("record: {Record}", scan.ToString());
        }
        _logger.LogInformation("Received request: {Command}", request.Command);
        tx.Commit();
        return base.CreateCommand(request, context);
    }
}
