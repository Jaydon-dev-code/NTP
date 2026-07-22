using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_RivetAndCrack : Factory4Workshop4_10LineBase
    {
        protected override string _serviceName => "四分厂4-10-铆接和裂纹";
        Tb_Factory4Workshop4_10Line_RivetAndCrackRepository _rivetAndCrackRepository;
        protected override Type _dataModelType { get; set; } =
            typeof(Tb_Factory4Workshop4_10Line_RivetAndCrack);

        public Factory4Workshop4_10Line_RivetAndCrack(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            Tb_Factory4Workshop4_10Line_RivetAndCrackRepository rivetAndCrackRepository,
            IHubContext chatHub
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _rivetAndCrackRepository = rivetAndCrackRepository;
        }

        protected override async Task<Result> HandshakeAsync()
        {
            _chatHub.Clients.All.IsOnlieRivetAndCrack = true;
            return Result.Success();
        }

        protected override async Task<Result> InitAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            return Result<object>.Success(new Tb_Factory4Workshop4_10Line_RivetAndCrack());
        }

        protected override Result IntiSetting()
        {
            throw new NotImplementedException();
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var data = (Tb_Factory4Workshop4_10Line_RivetAndCrack)interact.Data;
            _chatHub.Clients.All.Factory4Workshop4_10Line_RivetAndCrackDto =
                data.Adapt<Factory4Workshop4_10Line_RivetAndCrackDto>();

            if (string.IsNullOrEmpty(data.SN))
            {
                return;
            }

            await _rivetAndCrackRepository.InsertableAsync(data);

            await _summaryRepository.UpDataAsync(
                data.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                x => x.SN,
                _upCloName
            );
        }
    }
}
