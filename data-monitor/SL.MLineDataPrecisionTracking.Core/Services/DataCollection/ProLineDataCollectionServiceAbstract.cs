using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPOI.SS.Formula.Eval;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X9;
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
    public enum ServiceStatus
    {
        Stopped,
        Running,
        Paused,
    }

    public abstract class ProLineDataCollectionServiceAbstract
    {
        Tb_EquipmentRepository _equipmentRepository;

        public string ServiceName => _lineName;
        protected abstract string _lineName { get; set; }
        protected abstract Type DataModelType { get; }
        protected List<DevPlcPointMcDto> _lineReadPlcInfo;
        protected McpCommunication _mcp;

        DevPlcPointMcDto _plcCallPCCanCollectionPoint;
        DevPlcPointMcDto _pcCallPlcCollctionOk;

        // 服务状态管理
        public ServiceStatus Status { get; private set; } = ServiceStatus.Stopped;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _serviceTask;

        public ProLineDataCollectionServiceAbstract(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp
        )
        {
            _equipmentRepository = equipmentRepositor;
            _mcp = mcp;
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

        private static object GetValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            var prop = obj.GetType().GetProperty(fieldName);
            return prop?.CanRead == true ? prop.GetValue(obj) : null;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Status = ServiceStatus.Running;

                _lineReadPlcInfo = await InitPlcAddre();
                OtherInit();
                if (_lineReadPlcInfo == null || _lineReadPlcInfo.Count <= 0)
                {
                    Serilog.Log.Warning(
                        "[采集开始点位初始化]【{_lineName}】点位数据异常，本服务无法启动。",
                        _lineName
                    );
                    Status = ServiceStatus.Stopped;
                    return;
                }
                _plcCallPCCanCollectionPoint = _lineReadPlcInfo.First(x =>
                    x.PointName == "采集开始"
                );

                _pcCallPlcCollctionOk = _lineReadPlcInfo.First(x => x.PointName == "采集结束");
                _lineReadPlcInfo.Remove(_plcCallPCCanCollectionPoint);
                _lineReadPlcInfo.Remove(_pcCallPlcCollctionOk);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await CanCollection() is false || OtherCanCollection())
                        {
                            await Task.Delay(500, stoppingToken);
                            continue;
                        }

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
                        await Task.Delay(500, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不记录异常
                Serilog.Log.Information("[服务控制]【{_lineName}】服务已停止", _lineName);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(
                    "[点位数据初始化]{_lineName}数据初始化失败:{ex.Message}",
                    _lineName,
                    ex.Message
                );
            }
            finally
            {
                Status = ServiceStatus.Stopped;
            }
        }

        // 手动操作方法
        public void Start()
        {
            if (Status == ServiceStatus.Running)
            {
                Serilog.Log.Warning($"服务 {_lineName} 已经在运行中");
                return;
            }

            // 停止现有任务
            Stop();

            // 创建新的取消令牌
            _cancellationTokenSource = new CancellationTokenSource();

            // 启动服务
            _serviceTask = Task.Run(
                () => ExecuteAsync(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token
            );
        }

        public void Stop()
        {
            if (Status == ServiceStatus.Stopped)
            {
                return;
            }

            if (
                _cancellationTokenSource != null
                && !_cancellationTokenSource.IsCancellationRequested
            )
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = null;
            _serviceTask = null;
            Status = ServiceStatus.Stopped;
        }

        public void Restart()
        {
            Stop();
            // 延迟一下确保服务完全停止
            Task.Delay(1000).Wait();
            Start();
        }

        protected abstract void OtherInit();
        protected abstract bool OtherCanCollection();
        protected abstract Task<bool> InsterCollectionData(object data);

        protected virtual async Task<Result<object>> CollectionData()
        {
            var readValue = _mcp.Read(_lineReadPlcInfo);
            if (readValue.IsSuccess is false)
            {
                return Result<object>.Fail(readValue.Message);
            }
            object t = Activator.CreateInstance(DataModelType);
            var props = DataModelType.GetProperties();
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
                            prop.SetValue(t, readInfo.Value[0].ToString());
                        }
                        else
                        {
                            var val = readInfo.ReadFormula.StringCompute(
                                readInfo.Value[0].ToString()
                            );
                            prop.SetValue(t, val.ToString());
                        }
                    }
                    else
                    {
                        if (readInfo.DataType == TypeCode.String)
                        {
                            var val = string.Concat(readInfo.Value);
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
                    return Result<object>.Fail(
                        $"[点位数据初始化]{_lineName}反射数据失败:{ex.Message}"
                    );
                }
            }

            return Result<object>.Success(t);
        }

        protected virtual async Task CallPlcCollectionOK()
        {
            _pcCallPlcCollctionOk.Value = new List<object>() { true };
            await _mcp.WriteAsync(_pcCallPlcCollctionOk);
        }

        protected virtual async Task<bool> CanCollection()
        {
            var re = _mcp.Read(_plcCallPCCanCollectionPoint);
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
            if (linePoint is null)
            {
                return new List<DevPlcPointMcDto>();
            }
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
