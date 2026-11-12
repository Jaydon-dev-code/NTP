using System.IO;
using SL.ScanData.Models;
using SqlSugar;

namespace SL.ScanData.Data;

public class DbService
{
    private const string TableName = "scan_record";

    private readonly SqlSugarClient _db;

    public DbService()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScanData.db");
        // SQLite 连接时若文件不存在会自动创建数据库文件
        _db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"Data Source={dbPath}",
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
        EnsureTable();
    }

    /// <summary>
    /// 没有表则自动建表；已有表结构不兼容（如缺主键/自增）则保留数据重建。
    /// </summary>
    private void EnsureTable()
    {
        try
        {
            _db.CodeFirst.InitTables<ScanRecord>();
        }
        catch
        {
            if (!_db.DbMaintenance.IsAnyTable(TableName))
                throw;

            var cols = _db.DbMaintenance.GetColumnInfosByTableName(TableName, false);
            var idCol = cols.Find(c => c.DbColumnName == "Id");
            if (idCol != null && idCol.IsPrimarykey && idCol.IsIdentity)
                throw; // 表存在但失败原因不是结构问题，原样抛出

            // SQLite 不支持修改主键：先取数据，删表，按正确结构重建，再写回
            var oldData = _db.Queryable<ScanRecord>()
                .Select(x => new ScanRecord { Sn = x.Sn, ScanTime = x.ScanTime })
                .ToList();

            _db.DbMaintenance.DropTable(TableName);
            _db.CodeFirst.InitTables<ScanRecord>();

            if (oldData.Count > 0)
                _db.Insertable(oldData).ExecuteCommand();
        }
    }

    public ScanRecord? GetBySn(string sn)
    {
        return _db.Queryable<ScanRecord>().First(x => x.Sn == sn);
    }

    /// <summary>
    /// 插入记录；若 Sn 已存在（重复），返回 false 并给出之前录入的记录。
    /// </summary>
    public bool TryInsert(ScanRecord record, out ScanRecord? duplicate)
    {
        duplicate = GetBySn(record.Sn);
        if (duplicate != null) return false;

        try
        {
            _db.Insertable(record).ExecuteCommand();
            return true;
        }
        catch (Exception ex) when (ContainsUniqueViolation(ex))
        {
            duplicate = GetBySn(record.Sn);
            return false;
        }
    }

    public List<ScanRecord> GetLatest(int count)
    {
        return _db.Queryable<ScanRecord>()
            .OrderByDescending(x => x.Id)
            .Take(count)
            .ToList();
    }

    private static bool ContainsUniqueViolation(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException!)
        {
            if (e.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                e.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
