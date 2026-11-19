using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3
{
    public class Factory6Workshop6_3AssemblyLineA : Factory6Workshop6_3AssemblyLineAbstract
    {
        protected override string _lineName => "六分厂6-3装配A线";
        protected override Type _dataModelType => typeof(Tb_LineA);
        protected override string _trayPointName => "托盘号A";
        protected override string _serviceName => "六分厂6-3装配A线";
        protected virtual Type _passCodeEnumType => typeof(Assembly_A_LinePassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_A_LineNgCodeEnum);
        Dictionary<string, SugarColumn> _fieldAttributeDic;
        protected Tb_LineSummaryRepository _lineSummaryRepository;
        protected Tb_LineARepository _lineARepository;

        public Factory6Workshop6_3AssemblyLineA(
            Tb_EquipmentRepository equipmentRepositor,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            McpCommunication mcp,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            MesOrderOP mesOrderOP,
            MesApiOp mesApiOp
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository, mesOrderOP, mesApiOp)
        {
            _fieldAttributeDic = (new Tb_LineSummary()).GetFieldAttributeDict<SugarColumn>();
            _lineSummaryRepository = tb_LineSummaryRepository;
            _lineARepository = tb_LineARepository;
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_LineA)data).TrayNoA;
        }

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_LineA)interact.Data;

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

                Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);
                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelName = _models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelName ?? "";
                tb_LineSummary.LineATrayNo = lineData.TrayNoA;
                await _lineSummaryRepository.InsertableAsync(tb_LineSummary);

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

            await _lineARepository.InsertableAsync(lineData);
            return lineData;
        }

        private List<MesDataAcquisitionUploadItemDTO> BuildMesUploadItems(
            Tb_LineSummary tb_LineSummary
        )
        {
            var serialNo = tb_LineSummary.MarkingNo;
            var testResult = tb_LineSummary.Result.ToString();
            var workOrderNo = _mesOrderOP.OrderNo;
            var testTime = tb_LineSummary.RecordTime.ToString("yyyy-MM-dd HH:mm:ss");
            var re = new List<MesDataAcquisitionUploadItemDTO>();
            foreach (var item in _items63)
            {
                var val =
                    typeof(Tb_LineSummary).GetProperty(item.Field)?.GetValue(tb_LineSummary)
                    as string;
                if (string.IsNullOrEmpty(val))
                {
                    continue;
                }
                re.Add(
                    new MesDataAcquisitionUploadItemDTO
                    {
                        TenantId = MesHelper.Factory6TenantId,
                        PlantCode = "63",
                        EquipCode = item.EquipCode,
                        RouteCode = item.RouteCode,
                        SerialNo =
                            tb_LineSummary.Result == ResultEnum.OK ? serialNo : $"NG_63_A_{testTime}",
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
    }
}
