using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineA : Factory6Workshop6_3AssemblyLineAbstract
    {
        Tb_Factory6Workshop6_1AssemblyLineARepository _factory6Workshop6_1AssemblyLineARepository;
        Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository _factory6Workshop6_1AssemblyLineABSummaryRepository;

        public Factory6Workshop6_1AssemblyLineA(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            Tb_Factory6Workshop6_1AssemblyLineARepository tb_Factory6Workshop6_1AssemblyLineA,
            Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository factory6Workshop6_1AssemblyLineABSummaryRepository,
            MesOrderOP mesOrderOP,
            MesApiOp mesApiOp
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository, mesOrderOP, mesApiOp)
        {
            _factory6Workshop6_1AssemblyLineARepository = tb_Factory6Workshop6_1AssemblyLineA;
            _factory6Workshop6_1AssemblyLineABSummaryRepository =
                factory6Workshop6_1AssemblyLineABSummaryRepository;
            _fieldAttributeDic = (
                new Tb_Factory6Workshop6_1AssemblyLineABSummary()
            ).GetFieldAttributeDict<SugarColumn>();
            _sleepTimeSpan = TimeSpan.FromMilliseconds(200);
        }

        protected virtual Type _passCodeEnumType => typeof(Assembly_6Factory6_1ALine_PassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_6Factory6_1ALine_NgCodeEnum);
        protected override string _lineName => "六分厂6-1装配A线";
        Dictionary<string, SugarColumn> _fieldAttributeDic;
        protected override Type _dataModelType => typeof(Tb_Factory6Workshop6_1AssemblyLineA);
        protected override string _trayPointName => "托盘号A";

        protected override string _serviceName => "六分厂6-1装配A线";

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_Factory6Workshop6_1AssemblyLineA)interact.Data;

            if (lineData.ShieldStationA != "0")
            {
                if (int.TryParse(lineData.ShieldStationA, out var shieldStationA))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationA}) ");
                    foreach (var item in ParseBitEnum(shieldStationA, _passCodeEnumType))
                    {
                        stringBuilder.Append(item.Description + " ");
                    }
                    lineData.ShieldStationA = stringBuilder.ToString();
                }
            }

            if (lineData.NgCodeA != "0")
            {
                if (int.TryParse(lineData.NgCodeA, out var ngCodeA))
                {
                    lineData.NgCodeA =
                        $"({ngCodeA}) {GetEnumDescription(ngCodeA, _ngCodeEnumType)}";
                }

                Tb_Factory6Workshop6_1AssemblyLineABSummary tb_LineSummary =
                    new Tb_Factory6Workshop6_1AssemblyLineABSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);
                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelName = _models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelName ?? "";
                tb_LineSummary.LineATrayNo = lineData.TrayNoA;
                await _factory6Workshop6_1AssemblyLineABSummaryRepository.InsertableAsync(
                    tb_LineSummary
                );

                try
                {
                    var buildValue = BuildMesUploadItems(tb_LineSummary);
                    if (buildValue?.Count > 0)
                    {
                        await PushMes(buildValue);
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(
                        "[数据交互]【{_serverName}】推送数据到mes失败",
                        _serviceName
                    );
                }
            }
            await _factory6Workshop6_1AssemblyLineARepository.InsertableAsync(lineData);

            return lineData;
        }

        List<MesDataAcquisitionUploadItemDTO> BuildMesUploadItems(
            Tb_Factory6Workshop6_1AssemblyLineABSummary lneSummary
        )
        {
            var serialNo = lneSummary.MarkingNo;
            var testResult = lneSummary.Result.ToString();
            var workOrderNo = _mesOrderOP.OrderNo;
            var testTime = lneSummary.RecordTime.ToString("yyyy-MM-dd HH:mm:ss");
            var re = new List<MesDataAcquisitionUploadItemDTO>();
            foreach (var item in _items61)
            {
                var val =
                    typeof(Tb_Factory6Workshop6_1AssemblyLineABSummary)
                        .GetProperty(item.Field)
                        ?.GetValue(lneSummary) as string;
                if (val==null)
                {
                    continue;
                }
                re.Add(
                    new MesDataAcquisitionUploadItemDTO
                    {
                        TenantId = MesHelper.Factory6TenantId,
                        PlantCode = "61",
                        EquipCode = item.EquipCode,
                        RouteCode = item.RouteCode,
                        SerialNo =
                            lneSummary.Result == ResultEnum.OK ? serialNo : $"NG_61_A_{testTime}",
                        TestResult = testResult,
                        TestCode = _fieldAttributeDic[item.Field].ColumnDescription,
                        WorkOrderNo = workOrderNo,
                        TestValue = val,
                        TestTime = testTime,
                        Remark = testResult,
                    }
                );
            }
            return re;
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_Factory6Workshop6_1AssemblyLineA)data).TrayNoA;
        }
    }
}
