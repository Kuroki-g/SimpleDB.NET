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
        var tx = _db.NewTx();

        _logger.LogInformation("Received request: {Command}", request.Command);
        tx.Commit();
        return base.CreateCommand(request, context);
    }
}
