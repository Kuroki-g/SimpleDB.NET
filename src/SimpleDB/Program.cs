using SimpleDB.Services;
using SimpleDB.System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// see: https://learn.microsoft.com/ja-jp/aspnet/core/grpc/test-tools?view=aspnetcore-9.0
builder.Services.AddGrpcReflection();

var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
var db = new Database(dbConfig);
builder.Services.AddSingleton(db).AddSingleton<ISimpleDbConfig>(dbConfig);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<SqlService>();

IWebHostEnvironment env = app.Environment;
if (env.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"
);

app.Run();
