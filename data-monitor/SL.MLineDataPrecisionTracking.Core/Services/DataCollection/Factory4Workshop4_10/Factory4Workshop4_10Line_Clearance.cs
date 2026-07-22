using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_Clearance : Factory4Workshop4_10LineBase
    {
        protected override string _serviceName => "四分厂4-10-游隙";

        protected override Type _dataModelType { get; set; } =
            typeof(Tb_Factory4Workshop4_10Line_Clearance);

        public Tb_Factory4Workshop4_10Line_ClearanceRepository _clearanceRepository;
        Tb_Factory4Workshop4_10LineSummaryRepository _lineSummaryRepository;
        IHubContext _chatHub;

        public Factory4Workshop4_10Line_Clearance(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            IHubContext chatHub,
            Tb_Factory4Workshop4_10Line_ClearanceRepository factory4Workshop4_10Line_ClearanceRepository
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _clearanceRepository = factory4Workshop4_10Line_ClearanceRepository;
        }

        protected override async Task<Result> HandshakeAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result> InitAsync()
        {
            _chatHub.Clients.All.IsOnlieClearance = true;
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            return Result<object>.Success(new Tb_Factory4Workshop4_10Line_Clearance());
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var data = (Tb_Factory4Workshop4_10Line_Clearance)interact.Data;
            _chatHub.Clients.All.Factory4Workshop4_10Line_ClearanceDto =
                data.Adapt<Factory4Workshop4_10Line_ClearanceDto>();
            if (string.IsNullOrEmpty(data.SN))
            {
                return;
            }
            await _clearanceRepository.InsertableAsync(data);
            //第一个工位直接插入，工序不能乱序
            await _lineSummaryRepository.InsertableAsync(
                data.Adapt<Tb_Factory4Workshop4_10LineSummary>()
            );
        }

        protected override Result IntiSetting()
        {
            return Result.Success();
        }
    }
}
