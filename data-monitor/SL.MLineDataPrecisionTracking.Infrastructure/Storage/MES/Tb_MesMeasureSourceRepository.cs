using Serilog;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 三方库（SQL Server）测量数据仓储：动态列查询（除 image 外全列，中文列名做字典键）
    /// </summary>
    public class Tb_MesMeasureSourceRepository
    {
        private readonly ThirdPartyDb _thirdPartyDb;

        /// <summary>
        /// 非 image 列清单缓存（key=表名）
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<string>> _columnCache =
            new ConcurrentDictionary<string, List<string>>();

        public Tb_MesMeasureSourceRepository(ThirdPartyDb thirdPartyDb)
        {
            _thirdPartyDb = thirdPartyDb;
        }

        /// <summary>
        /// 三方库连通性检查
        /// </summary>
        public async Task<bool> PingAsync()
        {
            try
            {
                await _thirdPartyDb.Db.Ado.SqlQueryAsync<object>("SELECT 1");
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[MES推送] 三方库连通失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取表的非 image 列清单（按 ORDINAL_POSITION 排序，缓存）
        /// </summary>
        private async Task<List<string>> GetNonImageColumnsAsync(string tableName)
        {
            if (_columnCache.TryGetValue(tableName, out var cached))
            {
                return cached;
            }

            var cols = await _thirdPartyDb
                .Db.Ado.SqlQueryAsync<string>(
                    @"SELECT COLUMN_NAME
                        FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_NAME = @t AND DATA_TYPE <> 'image'
                       ORDER BY ORDINAL_POSITION",
                    new { t = tableName }
                );
            var list = cols?.ToList() ?? new List<string>();
            _columnCache[tableName] = list;
            return list;
        }

        /// <summary>
        /// 按复合游标查询新数据：测量时间 > 游标 或 (测量时间 = 游标 且 第二列 > 游标第二值)，
        /// 升序取前 take 条（默认1000）。返回字典列表（中文列名做键，DBNull→null）。
        /// </summary>
        /// <param name="lastSecondValue">上次第二游标值；为空表示首次（只按时间过滤）</param>
        public async Task<List<Dictionary<string, object>>> GetNewDataAsync(
            string tableName,
            string timeCol,
            string secondCol,
            bool secondIsInt,
            bool timeNullable,
            DateTime lastMeasureTime,
            string lastSecondValue,
            int take = 1000
        )
        {
            var columns = await GetNonImageColumnsAsync(tableName);
            if (columns.Count == 0)
            {
                Log.Warning($"[MES推送] 表 [{tableName}] 未找到非 image 列或表不存在");
                return new List<Dictionary<string, object>>();
            }

            // 中文列名/表名一律 [] 包裹，] 转义
            string Q(string name) => $"[{name.Replace("]", "]]")}]";
            var colSql = string.Join(",", columns.Select(Q));
            var time = Q(timeCol);
            var second = Q(secondCol);

            var parameters = new List<SugarParameter>
            {
                new SugarParameter("@time", lastMeasureTime),
            };

            string where;
            if (string.IsNullOrEmpty(lastSecondValue))
            {
                // 首次运行：只按时间过滤
                where = $"{time} > @time";
            }
            else if (secondIsInt)
            {
                if (!int.TryParse(lastSecondValue, out var secondInt))
                {
                    secondInt = int.MinValue;
                }
                parameters.Add(new SugarParameter("@second", secondInt));
                where =
                    $"({time} > @time OR ({time} = @time AND {second} > @second))";
            }
            else
            {
                parameters.Add(new SugarParameter("@second", lastSecondValue));
                where =
                    $"({time} > @time OR ({time} = @time AND {second} > @second))";
            }

            if (timeNullable)
            {
                where += $" AND {time} IS NOT NULL";
            }

            var sql =
                $"SELECT TOP ({take}) {colSql} FROM {Q(tableName)} WHERE {where} ORDER BY {time}, {second}";

            var dt = await _thirdPartyDb.Db.Ado.GetDataTableAsync(sql, parameters.ToArray());

            var rows = new List<Dictionary<string, object>>(dt.Rows.Count);
            foreach (DataRow dr in dt.Rows)
            {
                var dict = new Dictionary<string, object>(dt.Columns.Count);
                foreach (DataColumn col in dt.Columns)
                {
                    var v = dr[col];
                    dict[col.ColumnName] = v == DBNull.Value ? null : v;
                }
                rows.Add(dict);
            }
            return rows;
        }
    }
}
