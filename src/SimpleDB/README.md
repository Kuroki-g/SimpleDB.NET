# SimpleDB

Please use gRPC for connection

Local environment:

```bash
dotnet run --launch-profile https
```

Use SimpleDB.Provider for connection.
Sample is Client directory.

```bash
grpcurl -insecure localhost:7279 list sim_db.Sql
```

```bash
grpcurl -insecure -d '{ "command":"select * from T2" }' localhost:7279 sim_db.Sql.CreateCommand
```

```bash
grpcurl -insecure -d '{ "command":"create table T2(A int, B varchar(9))" }' localhost:7279 sim_db.Sql.ExecuteUpdateCmd
```

```bash
grpcurl -insecure -d "{ \"command\":\"insert into T2 (A, B) values (123, 'value1')\" }" localhost:7279 sim_db.Sql.ExecuteUpdateCmd
```
