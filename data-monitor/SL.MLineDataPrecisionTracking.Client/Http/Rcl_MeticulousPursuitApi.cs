using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class Rcl_MeticulousPursuitApi : BaseHttp
    {
        protected override string _controllerName => "Rcl_MeticulousPursuit";

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>分页查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> QueryablToPagee(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "QueryablToPagee",
                heatTreatmentDataQueryRequest
            );
        }

        /// <summary>
        /// 条码查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>条码查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> MarkingNoQuery(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "MarkingNoQuery",
                heatTreatmentDataQueryRequest
            );
        }

        /// <summary>
        /// 导出查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>导出查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> SaveQuery(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "SaveQuery",
                heatTreatmentDataQueryRequest
            );
        }
    }
}