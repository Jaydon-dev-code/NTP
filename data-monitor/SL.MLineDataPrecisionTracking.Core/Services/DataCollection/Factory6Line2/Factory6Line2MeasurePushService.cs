using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Line2
{
    /// <summary>
    /// 6分厂2号线 MES 测量数据推送服务：轮询三方 SQL Server 测量记录表。
    /// 游标只保存（表名, 上次最大测量时间）：初次（游标为空）用当前时间作起点，
    /// 查询成功后用本批数据的最大测量时间推进游标。
    /// 同名表只查询一次（多组 RouteCode/EquipCode 在同一查询里分组组装、合并推送）。
    /// </summary>
    public class Factory6Line2MeasurePushService : DataCollectionServiceAbstract
    {
        protected override string _serviceName => "6分厂2号线测量数据推送";

        /// <summary>三方表名（同时作为游标键）</summary>
        private const string AbsMeasureRecord = "ABS测量记录";
        private const string RunoutMeasureRecord = "跳动测量记录";
        private const string PressFitMeasureRecord = "压装测量记录";
        private const string ClearanceMeasureRecord = "游隙测量记录";
        private const string GreaseInjectionMeasureRecord = "注脂测量记录";
        private const string FlangeSortMeasureRecord = "法兰盘分选测量记录";
        private const string AssemblyMeasureRecord = "合套测量记录";

        /// <summary>
        /// 一个 RouteCode/EquipCode 分组：结果列 + 若干测量值列
        /// （op → RouteCode，zp → EquipCode）
        /// </summary>
        private class ColumnGroup
        {
            public string RouteCode;
            public string EquipCode;
            public string ResultColumn;
            public ValueColumn[] ValueColumns;
        }

        /// <summary>
        /// 测量值列：Column 为三方表实际列名，Description 为数据描述（用于 TestCode）。
        /// 不同表存在同名列（如"测量值"）但语义不同时，用 Description 区分；
        /// Description 为空时回退用列名。
        /// </summary>
        private class ValueColumn
        {
            /// <summary>三方表实际列名</summary>
            public string Column;

            /// <summary>数据描述（拼入 TestCode）；为空时用列名</summary>
            public string Description;

            public ValueColumn(string column, string description = null)
            {
                Column = column;
                Description = description;
            }

            /// <summary>TestCode 用的描述文本</summary>
            public string TestCodeText =>
                string.IsNullOrEmpty(Description) ? Column : Description;
        }

        /// <summary>
        /// 单张表的查询配置（同表多组只查一次）
        /// </summary>
        private class TableQuery
        {
            public string TableName;
            public ColumnGroup[] Groups;
        }

        /// <summary>
        /// 全部表的查询配置（来源：新建文本文档 (4).txt + ABS，同表名已合并）
        /// </summary>
        static readonly TableQuery[] QueryConfigs = new[]
        {
            // ABS 测量记录（测量时间列可空，WHERE 条件自动排除 NULL 行）
            new TableQuery
            {
                TableName = AbsMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP520",
                        EquipCode = "ZP32-17",
                        ResultColumn = "测量结果",
                        ValueColumns = new[]
                        {
                            new ValueColumn("齿数", "ABS齿数"),
                            new ValueColumn("工作气隙", "ABS工作气隙"),
                            new ValueColumn("Rising_TPD", "ABS_Rising_TPD"),
                            new ValueColumn("Failing_TPD", "ABS_Failing_TPD"),
                        },
                    },
                },
            },
            // 跳动测量记录：端跳组(1行) + 深度组(3行) 合并为一次查询
            new TableQuery
            {
                TableName = RunoutMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP500",
                        EquipCode = "ZP32-15",
                        ResultColumn = "分项结果_端跳",
                        ValueColumns = new[]
                        {
                            new ValueColumn("端跳值1","跳动端跳值1"),
                            new ValueColumn("端跳值2","跳动端跳值2"),
                        },
                    },
                    new ColumnGroup
                    {
                        RouteCode = "OP440",
                        EquipCode = "ZP32-12",
                        ResultColumn = "辅助_深度结果",
                        ValueColumns = new[] { new ValueColumn("压装深度", "压装深度") },
                    },
                },
            },
            new TableQuery
            {
                TableName = PressFitMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP490",
                        EquipCode = "ZP32-14",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("测量值", "压装测量值") },
                    },
                },
            },
            new TableQuery
            {
                TableName = ClearanceMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP420",
                        EquipCode = "ZP32-10",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("游隙值", "游隙值") },
                    },
                },
            },
            new TableQuery
            {
                TableName = GreaseInjectionMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP410",
                        EquipCode = "ZP32-09",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("测量值", "注脂测量值") },
                    },
                },
            },
            new TableQuery
            {
                TableName = FlangeSortMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP110",
                        EquipCode = "ZP32-02",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("测量值", "法兰盘分选测量值") },
                    },
                },
            },
            // 合套测量记录：外圈组(7行) + 内圈组(8行) 合并为一次查询
            new TableQuery
            {
                TableName = AssemblyMeasureRecord,
                Groups = new[]
                {
                    new ColumnGroup
                    {
                        RouteCode = "OP210",
                        EquipCode = "ZP32-06",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("外圈测量值", "外圈测量值") },
                    },
                    new ColumnGroup
                    {
                        RouteCode = "OP310",
                        EquipCode = "ZP32-08",
                        ResultColumn = "测量结果",
                        ValueColumns = new[] { new ValueColumn("内圈测量值", "内圈测量值") },
                    },
                },
            },
        };

        /// <summary>
        /// 游标初始化的表清单（每张表在 MesPushCursor 中按表名独立保存游标）。
        /// 必须声明在 QueryConfigs 之后（静态字段按文本顺序初始化）。
        /// </summary>
        static readonly string[] Tables = QueryConfigs.Select(c => c.TableName).Distinct().ToArray();

        private readonly ThirdPartyDb _thirdPartyDb;
        private readonly Tb_MesMeasureSourceRepository _sourceRepository;
        private readonly Tb_MesPushCursorRepository _cursorRepository;
        MesApiOp _mesApiOp;
        MesOrderOP _mesOrderOP;

        public Factory6Line2MeasurePushService(
            ThirdPartyDb thirdPartyDb,
            Tb_MesMeasureSourceRepository sourceRepository,
            Tb_MesPushCursorRepository cursorRepository,
            MesApiOp mesApiOp,
            MesOrderOP mesOrderOP
        )
        {
            _thirdPartyDb = thirdPartyDb;
            _sourceRepository = sourceRepository;
            _cursorRepository = cursorRepository;
            _mesApiOp = mesApiOp;
            _mesOrderOP = mesOrderOP;
        }

        protected override async Task<Result> InitAsync()
        {
            var intervalStr = ConfigurationManager.AppSettings["MesPushIntervalSeconds"];
            if (int.TryParse(intervalStr, out var seconds) && seconds > 0)
            {
                _sleepTimeSpan = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                _sleepTimeSpan = TimeSpan.FromSeconds(10);
            }

            if (_thirdPartyDb.Db == null)
            {
                return Result.Fail("未配置三方库连接（ThirdPartyConnection）");
            }

            // 逐表读游标；无记录 → 以当前时间初始化并落库（初次执行用当前时间作新数据标识）
            foreach (var table in Tables)
            {
                var cursor = await _cursorRepository.GetByKeyAsync(table);
                if (cursor == null)
                {
                    await _cursorRepository.UpsertAsync(table, DateTime.Now);
                    Serilog.Log.Information(
                        "[MES推送]【{_serverName}】表[{Table}]游标初始化为当前时间",
                        _serviceName,
                        table
                    );
                }
            }

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            //var ok = await _sourceRepository.PingAsync();
            //if (!ok)
            //{
            //    return Result.Fail("三方库连接失败");
            //}
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            // 按配置逐表查询推送（同表名只查一次，多组分组合并推送；ABS 也走统一逻辑）
            foreach (var cfg in QueryConfigs)
            {
                var cursor = await _cursorRepository.GetByKeyAsync(cfg.TableName);
                var startTime = cursor?.LastMeasureTime ?? DateTime.Now;

                var result = SelectByConfig(cfg, startTime, out var maxTime);
                if (maxTime > startTime)
                {
                    await _cursorRepository.UpsertAsync(cfg.TableName, maxTime);
                    Serilog.Log.Information(
                        "[MES推送]【{_serverName}】表[{Table}]游标推进至 {Time}",
                        _serviceName,
                        cfg.TableName,
                        maxTime
                    );
                }
                await PuseDateToMES(result);
            }
            return Result<object>.Success(null);
        }

        /// <summary>
        /// 按表配置查询（同表所有分组的列合并为一次 SELECT，最多1000条按测量时间升序），
        /// 每行按分组分别组装 DTO；out 参数返回本批数据的最大测量时间（无数据时等于入参 time）。
        /// </summary>
        private Result<List<MesDataAcquisitionUploadItemDTO>> SelectByConfig(
            TableQuery cfg,
            DateTime time,
            out DateTime maxTime
        )
        {
            maxTime = time;

            // 列清单：内码、测量时间 + 各分组的结果列/值列（去重）
            var cols = new List<string> { "内码", "测量时间" };
            foreach (var g in cfg.Groups)
            {
                if (!cols.Contains(g.ResultColumn))
                {
                    cols.Add(g.ResultColumn);
                }
                foreach (var vc in g.ValueColumns)
                {
                    if (!cols.Contains(vc.Column))
                    {
                        cols.Add(vc.Column);
                    }
                }
            }
            var colSql = string.Join(",", cols.Select(c => $"[{c}]"));

            var table = _thirdPartyDb.Db.Ado.GetDataTable(
                $"select top (1000) {colSql} from [{cfg.TableName}] where [测量时间] > @time order by [测量时间]",
                new SugarParameter("@time", time)
            );
            if (table.Rows.Count <= 0)
            {
                return Result<List<MesDataAcquisitionUploadItemDTO>>.Fail("采集数据为空");
            }

            var items = new List<MesDataAcquisitionUploadItemDTO>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var rowTime = Convert.ToDateTime(table.Rows[i]["测量时间"]);
                if (rowTime > maxTime)
                {
                    maxTime = rowTime;
                }
                var reTime = rowTime.ToString("yyyy-MM-dd HH:mm:ss");
                var sn = table.Rows[i]["内码"]?.ToString();
                if (string.IsNullOrEmpty(sn))
                {
                    continue; // 无内码的行跳过（游标仍按最大时间推进）
                }

                foreach (var g in cfg.Groups)
                {
                    var re = table.Rows[i][g.ResultColumn]?.ToString();
                    foreach (var vc in g.ValueColumns)
                    {
                        var val = table.Rows[i][vc.Column]?.ToString();
                        if (string.IsNullOrEmpty(val))
                        {
                            continue;
                        }
                        items.Add(
                            new MesDataAcquisitionUploadItemDTO
                            {
                                PlantCode = "61",
                                EquipCode = g.EquipCode,
                                RouteCode = g.RouteCode,
                                SerialNo = sn,
                                TestResult = re,
                                TestCode = "DB" + vc.TestCodeText,
                                WorkOrderNo = _mesOrderOP.OrderNo,
                                TestValue = val,
                                TestTime = reTime,
                            }
                        );
                    }
                }
            }

            if (items.Count == 0)
            {
                return Result<List<MesDataAcquisitionUploadItemDTO>>.Fail("采集数据为空");
            }
            return Result<List<MesDataAcquisitionUploadItemDTO>>.Success(items);
        }

        async Task PuseDateToMES(Result<List<MesDataAcquisitionUploadItemDTO>> data)
        {
            if (data.IsSuccess)
            {
                try
                {
                    await _mesApiOp.DataAcquisition(data.Data);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning($"【6-2】测量数据推送 异常：{ex.Message}");
                }
            }
        }

        protected override Task NotifyAsync(Result<object> interact)
        {
            // 推送已在 InteractAsync 中完成
            return Task.CompletedTask;
        }
    }
}
