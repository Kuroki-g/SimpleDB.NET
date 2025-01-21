# SimpleDB

Please use gRPC for connection

Local environment:

```bash
dotnet run --launch-profile https
```

Use SimpleDB.Provider for connection.
Sample is Client directory.

```bash
grpcurl -insecure -d '{ "command":"select * from t1" }' localhost:7279 sim_db.Sql.ExecuteQuery
```
