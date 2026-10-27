using System;
using System.Configuration;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 型号专用数据库连接包装（产品型号_当前产品型号表 / 产品型号表）。
    /// 与三方测量库（ThirdPartyDb）相互独立：无参构造自读 App.config 的 ModelDbConnection，
    /// 未配置时 Db 为 null（由调用方检查）。无参构造兼容 InfrastructureMiddleware 自动注册。
    /// </summary>
    public class ModelDb
    {
        /// <summary>
        /// 型号库 client；未配置连接串时为 null
        /// </summary>
        public ISqlSugarClient Db { get; }

        public ModelDb()
        {
            var conn = ConfigurationManager.AppSettings["ModelDbConnection"];
            if (string.IsNullOrWhiteSpace(conn))
            {
                Db = null;
                return;
            }

            var dbTypeStr = ConfigurationManager.AppSettings["ModelDbType"] ?? "SqlServer";
            DbType dbType;
            switch (dbTypeStr.ToLower())
            {
                case "mysql":
                    dbType = DbType.MySql;
                    break;
                case "postgresql":
                    dbType = DbType.PostgreSQL;
                    break;
                case "sqlserver":
                default:
                    dbType = DbType.SqlServer;
                    break;
            }

            Db = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = conn,
                    DbType = dbType,
                    IsAutoCloseConnection = true,
                }
            );
        }

        /// <summary>
        /// 查询当前产品型号名称：当前产品型号表取最新一行的型号编码，
        /// 关联产品型号表查出产品型号名称。未配置/无数据/异常返回空串。
        /// </summary>
        public async Task<string> GetCurrentModelAsync()
        {
            if (Db == null)
            {
                return string.Empty;
            }

            try
            {
                var table = await Db.Ado.GetDataTableAsync(
                    @"select top (1) b.[产品型号名称]
                        from [产品型号_当前产品型号表] a
                       inner join [产品型号表] b on a.[产品型号] = b.[产品型号编码]
                       where a.[产品型号] is not null
                       order by a.[ID] desc"
                );
                if (table.Rows.Count <= 0)
                {
                    return string.Empty;
                }
                return table.Rows[0][0]?.ToString()?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"[MES] 型号库查询当前型号失败：{ex.Message}");
                return string.Empty;
            }
        }
    }
}
