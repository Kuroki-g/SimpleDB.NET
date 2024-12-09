using TestHelper.Utils;

namespace SimpleDB.Feat.Test.Tx;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly string _dir;

    public IntegrationTestBase()
    {
        var randomStr = Helper.RandomString(12);
        _dir = $"./mock/{randomStr}";
        Helper.InitializeDir(_dir);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Helper.Delete(_dir);
    }
}
