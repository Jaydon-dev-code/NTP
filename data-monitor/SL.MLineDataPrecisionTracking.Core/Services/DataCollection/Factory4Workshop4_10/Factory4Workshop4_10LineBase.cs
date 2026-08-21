using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public abstract class Factory4Workshop4_10LineBase : DataCollectionServiceAbstract
    {

        protected List<DevPlcPointDto> _linePlcInfo;
        protected Tb_EquipmentRepository _equipmentRepository;
        protected McpCommunication _mcp;
        protected Tb_Factory4Workshop4_10LineSummaryRepository _summaryRepository;
        protected IHubContext _chatHub;
    
        public Factory4Workshop4_10LineBase(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication, Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository, IHubContext chatHub
        )
        {
            _equipmentRepository = tb_EquipmentRepository;
            _mcp = mcpCommunication;
            _summaryRepository=summaryRepository;
            _chatHub=chatHub;
        }

        protected override async Task<Result> InitAsync()
        {
            var readPlcPoint = await ReadPlcPoint();
            if (readPlcPoint.IsSuccess == false)
            {
                return readPlcPoint;
            }
            return await IntiSetting();
        }

        protected abstract Task<Result> IntiSetting();

        private async Task<Result> ReadPlcPoint()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == _serviceName
            );
            if (linePoint is null)
            {
                return Result.Fail("未找到设备点位信息");
            }

            _linePlcInfo = new List<DevPlcPointDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    _linePlcInfo.Add(
                        new DevPlcPointDto(
                            linePoint.DeviceName,
                            plcAddres.PointName,
                            plcLinkeInfo.IpAddress,
                            plcLinkeInfo.Port,
                            plcAddres.Area,
                            plcAddres.DataType.ToTypeCode(),
                            plcAddres.Address,
                            plcAddres.Length,
                            plcAddres.ReadFormula,
                            plcAddres.WriteFormula
                        )
                    );
                }
            }

            if (_linePlcInfo == null || _linePlcInfo.Count <= 0)
            {
                return Result.Fail("点位数据异常");
            }

            return Result.Success();
        }

    
    }
}
