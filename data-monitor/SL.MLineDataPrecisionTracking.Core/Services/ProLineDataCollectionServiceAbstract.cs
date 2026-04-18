using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPOI.SS.Formula.Eval;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public abstract class ProLineDataCollectionServiceAbstract<T>
        where T : class, new()
    {
        Tb_EquipmentRepository _equipmentRepository;

        protected abstract string _lineName { get; set; }
        protected List<DevPlcPointMcDto> _lineReadPlcInfo;
        protected McpCommunication _mcp;

        DevPlcPointMcDto _plcCallPCCanCollectionPoint;
        DevPlcPointMcDto _pcCallPlcCollctionOk;

        public ProLineDataCollectionServiceAbstract(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp
        )
        {
            _equipmentRepository = equipmentRepositor;
            _mcp = mcp;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _lineReadPlcInfo = await InitPlcAddre();

                _plcCallPCCanCollectionPoint = _lineReadPlcInfo.First(x =>
                    x.PointName == "采集开始"
                );

                _pcCallPlcCollctionOk = _lineReadPlcInfo.First(x => x.PointName == "采集结束");
                _lineReadPlcInfo.Remove(_plcCallPCCanCollectionPoint);
                _lineReadPlcInfo.Remove(_pcCallPlcCollctionOk);
                while (true)
                {
                    try
                    {
                        if (await CanCollection() is false)
                        {
                            continue;
                        }
                        //var isStrart = await CanCollection();
                        //Serilog.Log.Debug("[采集开始点位数据读取]【{_lineName}】{isStrart}", _lineName, isStrart);

                        var colRe = await CollectionData();
                        if (colRe.IsSuccess)
                        {
                            Serilog.Log.Debug(
                                "[采集开始点位数据读取]【{_lineName}】{@Data}",
                                _lineName,
                                colRe.Data
                            );
                            if (await InsterCollectionData(colRe.Data))
                            {
                                await CallPlcCollectionOK();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Warning(
                            "[点位数据初始化]{_lineName}数据读失败:{ex.Message}",
                            _lineName,
                            ex.Message
                        );
                    }
                    finally
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(
                    "[点位数据初始化]{_lineName}数据初始化失败:{ex.Message}",
                    _lineName,
                    ex.Message
                );
            }
        }

        protected abstract Task<bool> InsterCollectionData(T data);

        protected virtual async Task<Result<T>> CollectionData()
        {
            var readValue = await _mcp.ReadAsync(_lineReadPlcInfo);
            if (readValue.IsSuccess is false)
            {
                return Result<T>.Fail(readValue.Message);
            }
            T t = new T();
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<SugarColumn>();
                var columnDescription = attr?.ColumnDescription;
                if (attr?.ColumnDescription == null || attr?.ColumnDescription is null)
                {
                    continue;
                }
                try
                {
                    var readInfo = readValue.Data.First(x =>
                        x.PointName == attr?.ColumnDescription
                    );

                    if (readInfo.Length == 1)
                    {
                        if (readInfo.ReadFormula == null || readInfo.ReadFormula.Length == 0)
                        {
                            prop.SetValue(t, readInfo.Value[0]);
                        }
                        else
                        {
                            var val = readInfo.ReadFormula.StringCompute(
                                readInfo.Value[0].ToString()
                            );
                            prop.SetValue(t, val);
                        }
                    }
                    else
                    {
                        if (readInfo.DataType == TypeCode.String)
                        {
                            var val = string.Concat(readInfo.Value.Where(x => x is string));
                            prop.SetValue(t, val);
                        }
                        else
                        {
                            prop.SetValue(t, readInfo.Value.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(
                        "[点位数据初始化]{_lineName}反射数据失败:{ex.Message}",
                        _lineName,
                        ex.Message
                    );
                    return Result<T>.Fail($"[点位数据初始化]{_lineName}反射数据失败:{ex.Message}");
                }
            }

            return Result<T>.Success(t);
        }

        protected virtual async Task CallPlcCollectionOK()
        {
            _pcCallPlcCollctionOk.Value = new List<object>() { 1 };
            await _mcp.WriteAsync(_pcCallPlcCollctionOk);
        }

        protected virtual async Task<bool> CanCollection()
        {
            var re = await _mcp.ReadAsync(_plcCallPCCanCollectionPoint);
            if (re.IsSuccess is false || re.Data.Value[0].ObjToBool() is false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual async Task<List<DevPlcPointMcDto>> InitPlcAddre()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == _lineName
            );
            var re = new List<DevPlcPointMcDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    re.Add(
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
            return re;
        }
    }
}
