using SimpleDB.DataBuffer;
using SimpleDB.Logging;
using SimpleDB.Services;
using SimpleDB.Storage;
using SimpleDB.System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// see: https://learn.microsoft.com/ja-jp/aspnet/core/grpc/test-tools?view=aspnetcore-9.0
builder.Services.AddGrpcReflection();

var dbConfig = new SimpleDbConfig(blockSize: 4096, bufferSize: 4096, fileName: "simple.db");
using var fm = FileManager.GetInstance(
    new FileManagerConfig()
    {
        DbDirectory = dbConfig.FileName, // TODO: この定義が違うのでみなおす。
        FileName = dbConfig.FileName,
        BlockSize = dbConfig.BlockSize,
    }
);
builder.Services.AddSingleton<ISimpleDbConfig>(dbConfig).AddSingleton<IFileManager>(fm);
var lm = LogManager.GetInstance(FileManager.GetInstance(), dbConfig.LogFileName);
var bm = BufferManager.GetInstance(FileManager.GetInstance(), lm, dbConfig.BufferSize);
var db = new Database(dbConfig, FileManager.GetInstance(), lm, bm);
builder.Services.AddSingleton(db).AddSingleton<ILogManager>(lm).AddSingleton<IBufferManager>(bm);

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
