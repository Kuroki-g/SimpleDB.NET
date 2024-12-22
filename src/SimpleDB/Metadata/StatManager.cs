using System.Runtime.CompilerServices;
using SimpleDB.Structure;
using SimpleDB.Tx;

namespace SimpleDB.Metadata;

public sealed class StatManager : IStatManager
{
    private static readonly int REFRESH_THRESHOLD = 100;

    private int _calledCount = 0;

    private readonly ITableManager _tableManager;

    private Dictionary<string, StatInfo> _tableStats = [];

    public StatManager(ITableManager tableManager, ITransaction tx)
    {
        _tableManager = tableManager;
        RefreshStat(tx);
    }

    public StatInfo GetStatInfo(string tableName, Layout layout, ITransaction tx)
    {
        _calledCount += 1;
        if (_calledCount > REFRESH_THRESHOLD)
        {
            RefreshStat(tx);
        }
        var statInfo = _tableStats.GetValueOrDefault(tableName);
        if (statInfo is null)
        {
            statInfo = CalculateTableStats(tableName, layout, tx);
            _tableStats[tableName] = statInfo;
        }
        return statInfo;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static StatInfo CalculateTableStats(string tableName, Layout layout, ITransaction tx)
    {
        var recordCount = 0;
        var blockCount = 0;
        var ts = new TableScan(tx, tableName, layout);
        while (ts.Next())
        {
            recordCount += 1;
            blockCount = ts.GetRecordId().BlockNumber + 1;
        }
        ts.Close();

        return new StatInfo(blockCount, recordCount);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void RefreshStat(ITransaction tx)
    {
        _calledCount = 0; // 呼び出し回数をリセットする。
        _tableStats = [];
        var tCatLayout = _tableManager.GetLayout(TableManager.CATALOG_NAME_TABLE, tx);
        var tCat = new TableScan(tx, TableManager.CATALOG_NAME_TABLE, tCatLayout);
        while (tCat.Next())
        {
            var tableName = tCat.GetString(TableManager.CATALOG_NAME_TABLE);
            var layout = _tableManager.GetLayout(tableName, tx);
            _tableStats[tableName] = CalculateTableStats(tableName, layout, tx);
        }
        tCat.Close();
    }
}
