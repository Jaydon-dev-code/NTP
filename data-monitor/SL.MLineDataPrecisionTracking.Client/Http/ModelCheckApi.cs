using System;
using System.Configuration;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    /// <summary>
    /// 型号校验 API（对接本机 Service 的 modelCheck/*，仅状态查询供补显告警窗）
    /// </summary>
    public class ModelCheckApi : BaseHttpClient
    {
        protected override string _controllerName => "modelCheck";

        protected override Uri InitHttpUri() =>
            new Uri($"{ConfigurationManager.AppSettings["ServiceUri"]}/{_controllerName}/");

        /// <summary>
        /// 查询当前告警状态（启动/重连时调用，补显告警窗）
        /// </summary>
        public async Task<ApiResult<MesModelAlertStatusDto>> GetStatusAsync()
        {
            try
            {
                return await PostAsync<MesModelAlertStatusDto>("status", null);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"查询型号告警状态失败: {ex.Message}");
                return ApiResult<MesModelAlertStatusDto>.Fail(ex.Message);
            }
        }
    }
}
