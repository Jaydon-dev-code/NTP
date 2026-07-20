using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_Vib : DataCollectionServiceAbstract
    {
        protected override string _serviceName => "四分厂4-10-震动";

        protected override async Task<Result> HandshakeAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result> InitAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            return Result<object>.Success(new Tb_Factory4Workshop4_10Line_Vib());
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            return;
        }
    }
}
