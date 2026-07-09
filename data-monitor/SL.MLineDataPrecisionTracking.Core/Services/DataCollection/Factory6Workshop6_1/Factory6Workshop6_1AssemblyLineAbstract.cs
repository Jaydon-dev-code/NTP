using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public abstract class Factory6Workshop6_1AssemblyLineAbstract : DataCollectionServiceAbstract
    {
        protected Tb_EquipmentRepository _equipmentRepository;
        protected McpCommunication _mcp;
        protected Tb_LineARepository _lineARepository;
        protected Tb_LineSummaryRepository _lineSummaryRepository;
        protected Tb_ModelNoToNameRepository _modelNoToNameRepository;

        protected List<DevPlcPointMcDto> _lineReadPlcInfo;
        protected DevPlcPointMcDto _plcCallPCCanCollectionPoint;
        protected DevPlcPointMcDto _plcCallPCTrayNoPoint;
        /// <summary>
        /// 上次得托盘号
        /// </summary>
        protected string _lastTrayNoPoint;
        protected List<Tb_ModelNoToName> _models;
        
        protected abstract string _lineName { get; }
        /// <summary>
        /// 模型类型
        /// </summary>
        protected abstract Type _dataModelType { get; }
        /// <summary>
        /// 
        /// </summary>
        protected abstract string _trayPointName { get; }

        protected Factory6Workshop6_1AssemblyLineAbstract(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        )
        {
            _equipmentRepository = equipmentRepositor;
            _mcp = mcp;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
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

            _lineReadPlcInfo = new List<DevPlcPointMcDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    _lineReadPlcInfo.Add(
                        new DevPlcPointMcDto(
                            linePoint.DeviceName,
                            plcAddres.PointName,
                            plcLinkeInfo.IpAddress,
                            plcLinkeInfo.Port,
                            plcAddres.Area.ToPrefix(),
                            plcAddres.DataType.ToTypeCode(),
                            plcAddres.Address,
                            plcAddres.Length,
                            plcAddres.ReadFormula,
                            plcAddres.WriteFormula
                        )
                    );
                }
            }

            if (_lineReadPlcInfo == null || _lineReadPlcInfo.Count <= 0)
            {
                return Result.Fail("点位数据异常");
            }

            _plcCallPCCanCollectionPoint = _lineReadPlcInfo.First(x => x.PointName == "采集开始");
            _lineReadPlcInfo.Remove(_plcCallPCCanCollectionPoint);
            _plcCallPCTrayNoPoint = _lineReadPlcInfo.First(x => x.PointName == _trayPointName);
            _models = await _modelNoToNameRepository.QueryabletAsync(x => true);

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var canCol = _mcp.Read(_plcCallPCCanCollectionPoint);
            if (canCol.IsSuccess is false || canCol.Data.Value[0].ObjToBool() is false)
            {
                return Result.Fail("PLC未触发采集信号");
            }

            var trayRe = _mcp.Read(_plcCallPCTrayNoPoint);
            if (
                trayRe.IsSuccess is false
                || trayRe.Data.Value[0].ToString() == "0"
                || trayRe.Data.Value[0].ToString() == _lastTrayNoPoint
            )
            {
                return Result.Fail("托盘号无效或重复");
            }
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            var readValue = _mcp.Read(_lineReadPlcInfo);
            if (readValue.IsSuccess is false)
            {
                return Result<object>.Fail(readValue.Message);
            }

            return Expand.SugarColumnReflectAssign(readValue, _dataModelType);
        }

        protected void ABToSummary(
            object sourceA,
            object sourceB,
            object summary,
            List<string> ignoreFields = null
        )
        {
            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }

            var summaryProperties = summary
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.IsDefined(typeof(SugarColumn), true))
                .ToList();

            foreach (var prop in summaryProperties)
            {
                var fieldName = prop.Name;

                if (
                    ignoreFields.Any(ig => ig.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                )
                    continue;

                object value = GetValue(sourceA, fieldName);

                if (value == null)
                {
                    value = GetValue(sourceB, fieldName);
                }

                if (value != null)
                {
                    prop.SetValue(summary, value);
                }
            }
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
           var data= await InsterValue(interact);
            UpLastNo(data);
        }
        /// <summary>
        /// 更新上次对比得盘号
        /// </summary>
        /// <param name="data"></param>
        protected abstract void UpLastNo(object data);
        /// <summary>
        /// 插入数据到数据库
        /// </summary>
        /// <param name="interact"></param>
        /// <returns></returns>
        protected abstract Task<object> InsterValue(Result<object> interact);

        static object GetValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            var prop = obj.GetType().GetProperty(fieldName);
            return prop?.CanRead == true ? prop.GetValue(obj) : null;
        }
    }
}
