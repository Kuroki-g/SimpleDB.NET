using Grpc.Core;
using Microsoft.Extensions.Logging;
using SimDbGrpc;

namespace SimpleDB.Services;

public class SqlService(ILogger<SqlService> logger) : SimDbGrpc.Sql.SqlBase
{
    private readonly ILogger<SqlService> _logger = logger;

    public override Task<SqlResponse> CreateCommand(SqlRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received request: {Command}", request.Command);
        return base.CreateCommand(request, context);
    }
}
