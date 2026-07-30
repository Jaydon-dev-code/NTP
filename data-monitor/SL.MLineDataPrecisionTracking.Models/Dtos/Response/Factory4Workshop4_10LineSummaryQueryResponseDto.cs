using Mapster;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    public class Factory4Workshop4_10LineSummaryQueryResponseDto
    {
        public Factory4Workshop4_10LineSummaryQueryResponseDto(List<Tb_Factory4Workshop4_10LineSummary> list, int totalCount, int totalPage)
        {
            List = list.Adapt<List<Factory4Workshop4_10LineSummaryDto>>();
            TotalCount = totalCount;
            TotalPage = totalPage;
        }
        public List<Factory4Workshop4_10LineSummaryDto> List { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
