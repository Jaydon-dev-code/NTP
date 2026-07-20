using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_ABS : DataCollectionServiceAbstract
    {
        Tb_Factory4Workshop4_10Line_ClearanceRepository _clearanceRepository;
        Tb_Factory4Workshop4_10Line_RivetAndCrackRepository _rivetAndCrackRepository;
        Tb_Factory4Workshop4_10Line_VibRepository _vibRepository;
        Tb_Factory4Workshop4_10Line_ABSRepository _aBSRepository;
        Tb_Factory4Workshop4_10LineSummaryRepository _lineSummaryRepository;
        IHubContext _chatHub;
        protected override string _serviceName => "四分厂4-10-ABS";

        public Factory4Workshop4_10Line_ABS(
            Tb_Factory4Workshop4_10Line_ClearanceRepository clearanceRepository,
            Tb_Factory4Workshop4_10Line_RivetAndCrackRepository crackRepository,
            Tb_Factory4Workshop4_10Line_VibRepository vibRepository,
            Tb_Factory4Workshop4_10Line_ABSRepository aBSRepository,
            Tb_Factory4Workshop4_10LineSummaryRepository lineSummaryRepository,
            IHubContext chatHub
        )
        {
            _clearanceRepository = clearanceRepository;
            _rivetAndCrackRepository = crackRepository;
            _vibRepository = vibRepository;
            _aBSRepository = aBSRepository;
            _lineSummaryRepository = lineSummaryRepository;
            _chatHub = chatHub;
        }

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
            return Result<object>.Success(new Tb_Factory4Workshop4_10Line_ABS());
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var data = (Tb_Factory4Workshop4_10Line_ABS)interact.Data;
            if (string.IsNullOrEmpty(data.SN))
            {
                return;
            }

            await _aBSRepository.InsertableAsync(data);
            //发布当前数据
            _chatHub.Clients.All.Factory4Workshop4_10Line_ABS(data);
            Tb_Factory4Workshop4_10LineSummary lineSummary =
                new Tb_Factory4Workshop4_10LineSummary();
            Expand.ItemToSoure(
                lineSummary,
                null,
                (Tb_Factory4Workshop4_10Line_Clearance)interact.Data
            );
            //await _lineSummaryRepository.UpDataAsync(lineSummary, x => x.SN,);
        }
    }
}
