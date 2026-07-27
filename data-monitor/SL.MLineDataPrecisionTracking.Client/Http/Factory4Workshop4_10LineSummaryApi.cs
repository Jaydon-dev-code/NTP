using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class Factory4Workshop4_10LineSummaryApi : BaseHttp
    {
        protected override string _controllerName => "Factory4Workshop4_10Line";

        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> QueryablToPagee(
            Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            return await PostAsync<Factory4Workshop4_10LineSummaryQueryResponseDto>("QueryablToPagee", request);
        }

        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> SNQuery(
            Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            return await PostAsync<Factory4Workshop4_10LineSummaryQueryResponseDto>("SNQuery", request);
        }

        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> SaveQuery(
            Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            return await PostAsync<Factory4Workshop4_10LineSummaryQueryResponseDto>("SaveQuery", request);
        }
    }
}
