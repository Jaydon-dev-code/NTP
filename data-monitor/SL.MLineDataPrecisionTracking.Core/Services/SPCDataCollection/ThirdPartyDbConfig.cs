using System;
using System.Configuration;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection
{
    /// <summary>
    /// 三方/共享库「当前生产型号」同步配置。
    /// 配置项全部来自 App.config 的 appSettings，键前缀 ThirdPartyDb_。
    ///
    /// 最小可用只需两项：
    ///   1) ThirdPartyDb_ConnectionString — 三方库连接串
    ///   2) ThirdPartyDb_CurrentModelSql  — 返回单值（当前型号）的 SQL，允许连表
    ///
    /// 另有开关 Enabled、库类型 DbType、轮询间隔 SyncIntervalSeconds。
    /// </summary>
    public static class ThirdPartyDbConfig
    {
        /// <summary>
        /// 三方同步总开关。false 时同步服务直接不启动（服务列表备注也会提示）。
        /// </summary>
        public static bool Enabled =>
            bool.TryParse(ConfigurationManager.AppSettings["ThirdPartyDb_Enabled"], out var v) && v;

        /// <summary>三方/共享数据库连接串（与本地 DefaultConnection 无关）</summary>
        public static string ConnectionString =>
            ConfigurationManager.AppSettings["ThirdPartyDb_ConnectionString"] ?? "";

        /// <summary>三方库类型：PostgreSQL / SqlServer / MySql / Sqlite / Oracle，默认 PostgreSQL</summary>
        public static string DbTypeString =>
            ConfigurationManager.AppSettings["ThirdPartyDb_DbType"] ?? "PostgreSQL";

        /// <summary>
        /// 查询当前生产型号的 SQL，结果取第一行第一列（GetScalar）。
        /// 允许连表，例：
        ///   SELECT m.model_no FROM line_status s JOIN model m ON m.id=s.model_id WHERE s.line_id='L1'
        /// </summary>
        public static string CurrentModelSql =>
            ConfigurationManager.AppSettings["ThirdPartyDb_CurrentModelSql"] ?? "";

        /// <summary>轮询间隔（秒），默认 30；非法或 ≤0 时回落 30</summary>
        public static int SyncIntervalSeconds
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["ThirdPartyDb_SyncIntervalSeconds"], out var s) && s > 0)
                    return s;
                return 30;
            }
        }

        /// <summary>把 DbTypeString 解析为 SqlSugar 的 DbType（未知值按 PostgreSQL 处理）</summary>
        public static SqlSugar.DbType GetDbType()
        {
            switch (DbTypeString.ToLower())
            {
                case "mysql":
                    return SqlSugar.DbType.MySql;
                case "sqlserver":
                    return SqlSugar.DbType.SqlServer;
                case "sqlite":
                    return SqlSugar.DbType.Sqlite;
                case "oracle":
                    return SqlSugar.DbType.Oracle;
                case "postgresql":
                default:
                    return SqlSugar.DbType.PostgreSQL;
            }
        }

        /// <summary>是否已具备同步条件：连接串与型号 SQL 均非空</summary>
        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ConnectionString)
            && !string.IsNullOrWhiteSpace(CurrentModelSql);
    }
}
