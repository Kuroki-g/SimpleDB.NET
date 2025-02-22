using System.Text.Json;
using Grpc.Core;
using SimDbGrpc;
using SimpleDB.System;

namespace SimpleDB.Services;

public class SqlService(Database db, ILogger<SqlService> logger) : SimDbGrpc.Sql.SqlBase
{
    private readonly Database _db = db;
    private readonly ILogger<SqlService> _logger = logger;

    public override Task<SqlResponse> ExecuteQuery(SqlRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Command: {Command}", request.Command);
        var tx = _db.NewTx();
        // todo: とりあえずqueryのみ実行可能にしている
        var plan = _db.Planner.CreatePlan(request.Command, tx);
        var scan = plan.Open();
        var schema = plan.Schema;
        _logger.LogInformation("Schema: {Schema}", schema);
        var results = new List<Dictionary<string, string>>();
        while (scan.Next())
        {
            results.Add(
                schema.Fields.ToDictionary(
                    field => field,
                    field => scan.GetValue(field).ToString() ?? ""
                )
            );
        }
        _logger.LogInformation("Received request: {Command}", request.Command);
        tx.Commit();
        string jsonString = JsonSerializer.Serialize(
            results,
            new JsonSerializerOptions { WriteIndented = true }
        ); // 見やすくインデント

        return Task.FromResult(new SqlResponse { Message = $"Result: {jsonString}" });
    }

    public override Task<SqlResponse> ExecuteUpdateCmd(
        SqlRequest request,
        ServerCallContext context
    )
    {
        _logger.LogInformation("Command: {Command}", request.Command);
        var tx = _db.NewTx();
        // todo: とりあえずqueryのみ実行可能にしている
        var result = _db.Planner.ExecuteUpdateCmd(request.Command, tx);
        tx.Commit();
        return Task.FromResult(new SqlResponse { Message = $"Result: {result}" });
    }
}
