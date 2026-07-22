using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_ABS : Factory4Workshop4_10LineBase
    {
        Tb_Factory4Workshop4_10Line_ABSRepository _aBSRepository;
        protected override string _serviceName => "四分厂4-10-ABS";

        protected override Type _dataModelType { get; set; } =
            typeof(Tb_Factory4Workshop4_10Line_ABS);

        public Factory4Workshop4_10Line_ABS(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            IHubContext chatHub,
            Tb_Factory4Workshop4_10Line_ABSRepository aBSRepository
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _aBSRepository = aBSRepository;
        }

        protected override Result IntiSetting()
        {
            throw new NotImplementedException();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            _chatHub.Clients.All.IsOnlieABS = true;
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            return Result<object>.Success(new Tb_Factory4Workshop4_10Line_ABS());
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var data = (Tb_Factory4Workshop4_10Line_ABS)interact.Data;
            _chatHub.Clients.All.Factory4Workshop4_10Line_ABSDto =
                data.Adapt<Factory4Workshop4_10Line_ABSDto>();

            if (string.IsNullOrEmpty(data.SN))
            {
                return;
            }

            await _aBSRepository.InsertableAsync(data);

            await _summaryRepository.UpDataAsync(
                data.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                x => x.SN,
                _upCloName
            );
        }
    }
}
