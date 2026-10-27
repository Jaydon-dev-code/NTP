using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;

namespace SL.MLineDataPrecisionTracking.Core.Services.Mes
{
    /// <summary>
    /// 产线汇总（LineSummary）数据推 MES 帮助类：B线装配汇总入库后，
    /// 把汇总表中的测量字段组装为数采上报报文推送给 MES。
    /// 推送失败只记日志，不影响产线数据采集主流程。
    /// </summary>
    public static class MesLineSummaryPushHelper
    {
        /// <summary>
        /// TODO: RouteCode 真实值待 MES 侧确认，当前为假数据
        /// </summary>
        public const string FakeRouteCode = "OP999";

        /// <summary>
        /// TODO: EquipCode 真实值待 MES 侧确认，当前为假数据
        /// </summary>
        public const string FakeEquipCode = "ZP32-99";

        /// <summary>
        /// 推送一条产线汇总的数采数据。
        /// 无序列码或全部测量值为空时不推送。
        /// </summary>
        /// <param name="mesApiOp">MES 数采上报接口</param>
        /// <param name="mesOrderOP">订单操作（取执行中工单号，可空）</param>
        /// <param name="serialNo">序列码（MarkingNo）</param>
        /// <param name="testResult">检测结果（OK/NG）</param>
        /// <param name="testTime">检测时间（汇总记录时间）</param>
        /// <param name="measurements">测试项清单（TestCode, TestValue），空值项自动过滤</param>
        public static async Task PushAsync(
            MesApiOp mesApiOp,
            MesOrderOP mesOrderOP,
            string serialNo,
            string testResult,
            DateTime testTime,
            IEnumerable<(string TestCode, string TestValue)> measurements
        )
        {
            try
            {
                if (mesApiOp == null || string.IsNullOrWhiteSpace(serialNo))
                {
                    return;
                }

                var items = measurements
                    .Where(m => !string.IsNullOrWhiteSpace(m.TestValue))
                    .Select(
                        m =>
                            new MesDataAcquisitionUploadItemDTO
                            {
                                PlantCode = "61",
                                SerialNo = serialNo,
                                WorkOrderNo = mesOrderOP?.OrderNo ?? "",
                                TestCode = m.TestCode,
                                EquipCode = FakeEquipCode,
                                RouteCode = FakeRouteCode,
                                TestTime = testTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                TestValue = m.TestValue,
                                TestResult = testResult,
                            }
                    )
                    .ToList();

                if (items.Count == 0)
                {
                    return;
                }

                await mesApiOp.DataAcquisition(items);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"【产线汇总】MES数采推送异常：{ex.Message}");
            }
        }
    }
}
