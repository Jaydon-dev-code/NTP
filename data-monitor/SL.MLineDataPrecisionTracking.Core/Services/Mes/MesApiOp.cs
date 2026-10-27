using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Http;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;

namespace SL.MLineDataPrecisionTracking.Core.Services.Mes
{
    public class MesApiOp : BaseHttp
    {
        protected override string _controllerName => "";

        protected override Uri InitHttpUri() =>
            new Uri($"{ConfigurationManager.AppSettings["MesPushUri"]}/");

        public async Task<bool> DataAcquisition(
            List<MesDataAcquisitionUploadItemDTO> mesDataAcquisitionUploads
        )
        {
            ApiResult<dynamic> re = await PostOtherAsync<dynamic>(
                "/tracing/data_acquisition/upload",
                mesDataAcquisitionUploads
            );
            return re.Data.code == 200;
        }
    }
}
