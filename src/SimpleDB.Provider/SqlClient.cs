namespace SimpleDB.Provider;

using Grpc.Net.Client;
using SimDbGrpc;

public class SqlClient : IDisposable
{
    private readonly GrpcChannel? _channel;

    public SqlClient(string address)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        // The port number must match the port of the gRPC server.
        _channel = GrpcChannel.ForAddress(
            address,
            new GrpcChannelOptions { HttpHandler = handler }
        );
        var client = new Sql.SqlClient(_channel);
        var req = new SqlRequest { Command = "command" };
        var res = client.CreateCommand(req);
    }

    public void Execute(string command)
    {
        var client = new Sql.SqlClient(_channel);
        var req = new SqlRequest { Command = command };
        var res = client.CreateCommand(req);

        Console.WriteLine(res?.Message);
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
