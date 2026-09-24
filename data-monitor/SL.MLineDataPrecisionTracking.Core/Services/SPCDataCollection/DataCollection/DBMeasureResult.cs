using System;
using System.Collections.Generic;
using SqlSugar;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection
{
    /// <summary>
    /// 数据库测量结果采集 — 用 SqlSugar 连接 SQL Server 查询测量数据。
    ///
    /// 查询语句由 4 个字段拼接：
    ///   SELECT {MeasureValueField}, {MeasureResultField} FROM {TableName} WHERE {TimeFieldName} > @lastTime
    /// WHERE 条件时间为 Tb_SPC_CollectionSource.LastCollectionTime（null 则用当前时间）。
    /// 查询后更新内存 _lastQueryTime 和数据库 LastCollectionTime。
    /// </summary>
    public class DBMeasureResult : SPCDataCollectionAbstract
    {
        Tb_SPC_CollectionSource _source;
        SqlSugarClient _db;
        DateTime _lastQueryTime;

        public override string _featureName { get; set; }
        public override SPCDataSource DataSource => SPCDataSource.Database;
        public override string Description => "数据库测量结果采集";

        /// <summary>
        /// 初始化：objs[0] = 特性名称(string)，objs[1] = Tb_SPC_CollectionSource。
        /// 创建 SqlSugar SQL Server 连接，设置查询时间。
        /// </summary>
        protected override void InitCollectionInfo(object[] objs)
        {
            _source = objs.Length > 1 ? objs[1] as Tb_SPC_CollectionSource : null;
            if (_source == null) return;

            // 创建 SqlSugar SQL Server 连接
            _db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = _source.ConnectionString,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true,
            });

            // 初始查询时间：取数据库记录的 LastCollectionTime，null 则用当前时间
            _lastQueryTime = _source.LastCollectionTime ?? DateTime.Now;
        }

        /// <summary>
        /// 执行采集：查询自上次采集时间之后的新测量数据。
        /// 查询后更新内存 _lastQueryTime 和数据库 LastCollectionTime。
        /// </summary>
        public override void Execute()
        {
            if (_source == null || _db == null) return;

            try
            {
                // 拼接查询语句
                var sql = $"SELECT [{_source.MeasureValueField}], [{_source.MeasureResultField}] " +
                          $"FROM [{_source.TableName}] " +
                          $"WHERE [{_source.TimeFieldName}] > @lastTime";

                var pars = new List<SugarParameter>
                {
                    new SugarParameter("@lastTime", _lastQueryTime),
                };

                var dt = _db.Ado.GetDataTable(sql, pars.ToArray());

                if (dt.Rows.Count > 0)
                {
                    // TODO: 查询结果判定逻辑（先空着）
                    // 遍历 dt.Rows，读取测量值和测量结果字段进行处理
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var measureValue = dt.Rows[i][_source.MeasureValueField];
                        var measureResult = dt.Rows[i][_source.MeasureResultField];
                        // 空着
                    }

                    // 更新内存查询时间为当前时间
                    _lastQueryTime = DateTime.Now;

                    // 更新数据库 LastCollectionTime
                    _source.LastCollectionTime = _lastQueryTime;
                    _db.Updateable(_source).ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(
                    "[SPC数据库采集]【{Name}】查询异常: {Message}",
                    _featureName, ex.Message
                );
            }
        }
    }
}
