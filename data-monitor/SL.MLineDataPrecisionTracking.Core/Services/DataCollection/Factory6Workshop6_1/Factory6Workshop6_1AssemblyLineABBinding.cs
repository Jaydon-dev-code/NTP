using System;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    /// <summary>
    /// 六分厂6-1装配 AB托盘绑定 专用读取线程
    /// 独立线程持续读取 A托盘绑定 / B托盘绑定 点位，与A/B线采集线程并行：
    /// 把 b 线托盘号 更新到 Tb_Factory6Workshop6_1AssemblyLineA 数据库，供B线采集时查询
    /// </summary>
    public class Factory6Workshop6_1AssemblyLineABBinding : DataCollectionServiceAbstract
    {
        protected  string _lineName = "六分厂6-1装配B线";
        private const string _aBindPointName = "A托盘绑定";
        private const string _bBindPointName = "B托盘绑定";

        protected override string _serviceName => "六分厂6-1AB托盘绑定";

        private readonly Tb_EquipmentRepository _equipmentRepository;
        private readonly Tb_Factory6Workshop6_1AssemblyLineARepository _aLineRepository;
        private readonly McpCommunication _mcp;

        private DevPlcPointDto _aBindPoint;
        private DevPlcPointDto _bBindPoint;

        private string _lastATrayNo;
        private string _lastBTrayNo;

        public Factory6Workshop6_1AssemblyLineABBinding(
            Tb_EquipmentRepository equipmentRepository,
            Tb_Factory6Workshop6_1AssemblyLineARepository aLineRepository,
            McpCommunication mcp
        )
        {
            _equipmentRepository = equipmentRepository;
            _aLineRepository = aLineRepository;
            _mcp = mcp;
            _sleepTimeSpan = TimeSpan.FromMilliseconds(200);
        }

        protected override async Task<Result> InitAsync()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == _lineName
            );
            if (linePoint is null)
            {
                return Result.Fail("未找到设备点位信息");
            }

            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    var point = new DevPlcPointDto(
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
                    );

                    if (plcAddres.PointName == _aBindPointName)
                    {
                        _aBindPoint = point;
                    }
                    else if (plcAddres.PointName == _bBindPointName)
                    {
                        _bBindPoint = point;
                    }
                }
            }

            if (_aBindPoint is null || _bBindPoint is null)
            {
                return Result.Fail($"未找到点位：{_aBindPointName} / {_bBindPointName}");
            }

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            var aResult = _mcp.Read(_aBindPoint);
            var bResult = _mcp.Read(_bBindPoint);
            if (aResult.IsSuccess && bResult.IsSuccess)
            {
                var aTrayNo = aResult.Data.Value?.FirstOrDefault()?.ToString() ?? "0";
                var bTrayNo = bResult.Data.Value?.FirstOrDefault()?.ToString() ?? "0";

               
                // 条件：a 托盘号非0/空 且 与上次数据不同
                if (
                    aTrayNo != "0"
                    && !string.IsNullOrWhiteSpace(aTrayNo)
                    && aTrayNo != _lastATrayNo
                    && bTrayNo != "0"
                    && !string.IsNullOrWhiteSpace(bTrayNo)
                    && bTrayNo != _lastBTrayNo
                )
                {
                    // 通过 a 托盘号查 a 线最新未使用的数据，把 b 线托盘号更新到数据库
                    var aLineInfo = await _aLineRepository.QueryableFirstAsync(
                        x => x.TrayNoA == aTrayNo && x.IsUsed == false,
                        o => o.RecordTime
                    );
                    if (aLineInfo != null)
                    {
                        aLineInfo.TrayNoB = bTrayNo;
                        await _aLineRepository.UpdateableAsync(aLineInfo);
                    }
                    _lastATrayNo = aTrayNo;
                    _lastBTrayNo = bTrayNo;
                }
               
             
            }

            return Result<object>.Success(null);
        }

        protected override async Task NotifyAsync(Result<object> interact) { }
    }
}
