using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_Clearance : DataCollectionServiceAbstract
    {
        protected override string _serviceName => "四分厂4-10-游隙";
        public Tb_Factory4Workshop4_10Line_ClearanceRepository _clearanceRepository;
        Tb_Factory4Workshop4_10LineSummaryRepository _lineSummaryRepository;
        IHubContext _chatHub;

        public Factory4Workshop4_10Line_Clearance(
            Tb_Factory4Workshop4_10Line_ClearanceRepository factory4Workshop4_10Line_ClearanceRepository, Tb_Factory4Workshop4_10LineSummaryRepository lineSummaryRepository, IHubContext chatHub
        )
        {
            _clearanceRepository = factory4Workshop4_10Line_ClearanceRepository;
            _lineSummaryRepository= lineSummaryRepository;
            _chatHub= chatHub;
        }

 

        protected override async Task<Result> HandshakeAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result> InitAsync()
        {
            return Result.Success();
        }

        protected override Task<Result<object>> InteractAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            await _clearanceRepository.InsertableAsync(
                (Tb_Factory4Workshop4_10Line_Clearance)interact.Data
            );
            Tb_Factory4Workshop4_10LineSummary lineSummary=new Tb_Factory4Workshop4_10LineSummary();
            Expand.ItemToSoure(lineSummary,null, (Tb_Factory4Workshop4_10Line_Clearance)interact.Data);
            //第一个工位直接插入，工序不能乱序
           await _lineSummaryRepository.InsertableAsync(lineSummary);

        }
    }
}
