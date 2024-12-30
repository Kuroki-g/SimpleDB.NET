namespace SimpleDB.Provider;

using Grpc.Net.Client;
using GrpcGreeter;

public class SqlClient
{
    public string GetMessage() => "Hello from SimpleDB.Provider";

    public void Sample()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        // The port number must match the port of the gRPC server.
        using var channel = GrpcChannel.ForAddress(
            "https://localhost:7279",
            new GrpcChannelOptions { HttpHandler = handler }
        );
        var client = new Greeter.GreeterClient(channel);
    }
}
