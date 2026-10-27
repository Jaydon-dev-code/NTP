using System;
using System.Configuration;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 三方数据库（SQL Server）连接包装。无参构造自读 App.config，
    /// 未配置 ThirdPartyConnection 时 Db 为 null（由调用方检查）。
    /// 无参构造是为了兼容 InfrastructureMiddleware 的自动注册（避免构造参数无法解析）。
    /// </summary>
    public class ThirdPartyDb
    {
        /// <summary>
        /// 三方库 client；未配置连接串时为 null
        /// </summary>
        public ISqlSugarClient Db { get; }

        public ThirdPartyDb()
        {
            var conn = ConfigurationManager.AppSettings["ThirdPartyConnection"];
            if (string.IsNullOrWhiteSpace(conn))
            {
                Db = null;
                return;
            }

            var dbTypeStr = ConfigurationManager.AppSettings["ThirdPartyDbType"] ?? "SqlServer";
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
    }
}
