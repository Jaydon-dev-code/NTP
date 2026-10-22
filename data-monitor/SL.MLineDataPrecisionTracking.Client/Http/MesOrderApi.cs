using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    /// <summary>
    /// MES 工单 API（对接本机 Service 的 api/order/*）
    /// </summary>
    public class MesOrderApi : BaseHttpClient
    {
        protected override string _controllerName => "order";
        protected override Uri InitHttpUri() =>
         new Uri($"{ConfigurationManager.AppSettings["ServiceUri"]}/{_controllerName}/");

        /// <summary>
        /// 接口一：新增工单
        /// </summary>
        public async Task<ApiResult> ReleaseAsync(MesOrderReleaseDto dto)
        {
            try
            {
                return await PostAsync("release", dto);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error($"MES新增工单失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 接口二：更新/删除工单
        /// </summary>
        public async Task<ApiResult> UpdateAsync(MesOrderUpdateDto dto)
        {
            try
            {
                return await PostAsync("update", dto);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error($"MES更新工单失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 查询工单列表
        /// </summary>
        public async Task<ApiResult<List<Tb_MesOrder>>> QueryAsync()
        {
            try
            {
                return await PostAsync<List<Tb_MesOrder>>("query", null);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error($"MES查询工单失败: {ex.Message}");
                return ApiResult<List<Tb_MesOrder>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 订单状态切换（0待执行 / 1执行中）
        /// </summary>
        public async Task<ApiResult> SetStatusAsync(string workOrderNo, int status)
        {
            try
            {
                return await PostAsync("setStatus", new MesOrderStatusDto
                {
                    WorkOrderNo = workOrderNo,
                    Status = status,
                });
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error($"MES切换工单状态失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}
