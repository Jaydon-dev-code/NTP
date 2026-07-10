using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class Factory6Workshop6_1AssemblyLineApi : BaseHttp
    {
        protected override string _controllerName => "Factory6Workshop6_1AssemblyLine";

        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> QueryablToPagee(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<Factory6Workshop6_1AssemblyLineResponseDto>(
                "QueryablToPagee",
                lineSummaryQueryRequest
            );
        }

        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> MarkingNoQuery(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<Factory6Workshop6_1AssemblyLineResponseDto>(
                "MarkingNoQuery",
                lineSummaryQueryRequest
            );
        }

        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> SaveQuery(
            LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            return await PostAsync<Factory6Workshop6_1AssemblyLineResponseDto>(
                "SaveQuery",
                lineSummaryQueryRequest
            );
        }
    }
}
