using SimpleDB.Metadata;
using SimpleDB.SqlParser;
using SimpleDB.SqlParser.Grammar;
using SimpleDB.Tx;

namespace SimpleDB.Plan;

public class BasicUpdatePlanner(IMetadataManager mm) : IUpdatePlanner
{
    private readonly IMetadataManager _mm = mm;

    // Class implementation goes here
    public int ExecuteCreateTable(CreateTable cmd, ITransaction tx)
    {
        throw new NotImplementedException();
    }

    public int ExecuteDelete(Delete cmd, ITransaction tx)
    {
        throw new NotImplementedException();
    }

    public int ExecuteInsert(Insert cmd, ITransaction tx)
    {
        throw new NotImplementedException();
    }
}
