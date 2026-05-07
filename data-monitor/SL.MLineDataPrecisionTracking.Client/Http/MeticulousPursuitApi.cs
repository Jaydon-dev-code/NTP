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
    public class MeticulousPursuitApi : BaseHttp
    {
        protected override string _controllerName => "MeticulousPursuit";

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="lineSummaryQueryRequest">查询请求参数</param>
        /// <returns>分页查询结果</returns>
        public async Task<ApiResult<LineSummaryQueryResponseDto>> QueryablToPagee(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<LineSummaryQueryResponseDto>(
                "QueryablToPagee",
                lineSummaryQueryRequest
            );
        }

        /// <summary>
        /// 条码查询
        /// </summary>
        /// <param name="lineSummaryQueryRequest">查询请求参数</param>
        /// <returns>条码查询结果</returns>
        public async Task<ApiResult<LineSummaryQueryResponseDto>> MarkingNoQuery(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<LineSummaryQueryResponseDto>(
                "MarkingNoQuery",
                lineSummaryQueryRequest
            );
        }

        /// <summary>
        /// 导出查询
        /// </summary>
        /// <param name="lineSummaryQueryRequest">查询请求参数</param>
        /// <returns>导出查询结果</returns>
        public async Task<ApiResult<LineSummaryQueryResponseDto>> SaveQuery(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<LineSummaryQueryResponseDto>(
                "SaveQuery",
                lineSummaryQueryRequest
            );
        }
    }
}