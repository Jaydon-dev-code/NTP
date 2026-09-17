using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar.Extensions;
using IClientProxy = Microsoft.AspNet.SignalR.Hubs.IClientProxy;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_Clearance : Factory4Workshop4_10LineBase
    {
        protected override string _serviceName => "四分厂4-10-游隙";

        public Tb_Factory4Workshop4_10Line_ClearanceRepository _clearanceRepository;

        /// <summary>
        /// 下发序列码给plc的点位
        /// </summary>
        protected DevPlcPointDto _issueSancInfoPoint;
        Socket _serverSocket;
        DevPlcPointDto _clearance1OK;
        DevPlcPointDto _clearance1NG;
        DevPlcPointDto _clearance1SN;

        DevPlcPointDto _clearance2OK;
        DevPlcPointDto _clearance2NG;
        DevPlcPointDto _clearance2SN;
        List<DevPlcPointDto> _clearancResultPlcInfo;

        List<DevPlcPointDto> _clearance1PlcInfo;
        List<DevPlcPointDto> _clearance2PlcInfo;

        DevPlcPointDto _positiveGapPlcInfo;
        DevPlcPointDto _lowerLoadPlcInfo;
        DevPlcPointDto _uperLoadPlcInfo;
        DevPlcPointDto _offsetPlcInfo;
        DevPlcPointDto _beforePressInPlcInfo;
        DevPlcPointDto _afterPressInPlcInfo;
        DevPlcPointDto _gapPlcInfo;

        byte _clearance1Re;
        byte _clearance1ReTmp;
        byte _clearance2Re;
        byte _clearance2ReTmp;
        string _lastClearance1SN;
        string _lastcanInfo;
        string _lastClearance2SN;

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

        protected override async Task<Result> IntiSetting()
        {
            #region  检测点位
            _clearance1OK = _linePlcInfo.First(x => x.PointName == "游隙1工位OK");
            _clearance1NG = _linePlcInfo.First(x => x.PointName == "游隙1工位NG");
            _clearance2OK = _linePlcInfo.First(x => x.PointName == "游隙2工位OK");
            _clearance2NG = _linePlcInfo.First(x => x.PointName == "游隙2工位NG");
            _clearancResultPlcInfo = new()
            {
                _clearance1OK,
                _clearance1NG,
                _clearance2OK,
                _clearance2NG,
            };
            #endregion

            #region  1工位
            _clearance1SN = _linePlcInfo.First(x => x.PointName == "游隙1工位SN");
            _positiveGapPlcInfo = _linePlcInfo.First(x => x.PointName == "正间隙");
            _lowerLoadPlcInfo = _linePlcInfo.First(x => x.PointName == "下负荷");
            _uperLoadPlcInfo = _linePlcInfo.First(x => x.PointName == "上负荷");
            _clearance1PlcInfo = new List<DevPlcPointDto>()
            {
                _clearance1SN,
                _positiveGapPlcInfo,
                _lowerLoadPlcInfo,
                _uperLoadPlcInfo,
            };
            #endregion

            #region  2工位
            _clearance2SN = _linePlcInfo.First(x => x.PointName == "游隙2工位SN");
            _offsetPlcInfo = _linePlcInfo.First(x => x.PointName == "偏移量");
            _beforePressInPlcInfo = _linePlcInfo.First(x => x.PointName == "压入前");
            _afterPressInPlcInfo = _linePlcInfo.First(x => x.PointName == "压入后");
            _gapPlcInfo = _linePlcInfo.First(x => x.PointName == "间隙");
            _clearance2PlcInfo = new List<DevPlcPointDto>()
            {
                _clearance2SN,
                _offsetPlcInfo,
                _beforePressInPlcInfo,
                _afterPressInPlcInfo,
                _gapPlcInfo,
            };
            _issueSancInfoPoint = _linePlcInfo.First(x => x.PointName == "下发游隙SN");
            _issueSancInfoPoint.Value = new List<object>() { "" };
            #endregion
            Task.Run(() => ScanSocket());
            return Result.Success();
        }

        private void ScanSocket()
        {
            if (
                int.TryParse(
                    ConfigurationManager.AppSettings["Factory4_10_ClearanceScanPort"],
                    out int factory4_10ScanPort
                ) == false
            )
            {
                Serilog.Log.Warning("[扫描枪数据推送]{_serverName}端口解析失败。", _serviceName);
                return;
            }
            _serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, factory4_10ScanPort);
            _serverSocket.Bind(ep);
            _serverSocket.Listen(5);
            // 循环等待读码器接入
            try
            {
                while (true)
                {
                    Socket client = _serverSocket.Accept();
                    ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieClearanceScan", true);

                    // 新开线程持续接收条码
                    new Thread(() => ReceiveBarcodeLoop(client)).Start();
                }
            }
            catch (ObjectDisposedException)
            {
                Serilog.Log.Information(
                    "[扫描枪数据推送]{_serviceName}:Socket已关闭，停止监听。",
                    _serviceName
                );
            }
            catch (SocketException ex)
            {
                Serilog.Log.Warning(
                    "[扫描枪数据推送]{_serviceName}监听异常:{ex.Message}",
                    _serviceName,
                    ex.Message
                );
            }
        }

        void ReceiveBarcodeLoop(Socket client)
        {
            byte[] buffer = new byte[100];
            try
            {
                while (true)
                {
                    int len = client.Receive(buffer);
                    ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieClearanceScan", true);
                    if (len <= 0)
                    {
                        Serilog.Log.Warning(
                            "[扫描枪数据推送]{_serverName}通讯没数据。",
                            _serviceName
                        );

                        break;
                    }
                    var soureLen = int.Parse(buffer.BytesToAscii(4)) - 4;
                    var soureByte = buffer.RemoveStartBytes(4);
                    var canInfo = soureByte.BytesToAscii(soureLen);
                    if (_lastcanInfo != canInfo)
                    {
                        _issueSancInfoPoint.Value[0] = canInfo;
                        _mcp.Write(_issueSancInfoPoint);
                        _lastcanInfo = canInfo;
                    }
                    //if (!Expand.IsRunningInMSTest())
                    //{

                    //}
                    //else
                    //{
                    //    string reMesg = "TestMsgIs" + ccanInfo;
                    //    byte[] body = Encoding.UTF8.GetBytes(reMesg);
                    //    client.Send(body);
                    //}
                }
            }
            catch (SocketException ex)
            {
                Serilog.Log.Warning(
                    "[扫描枪数据推送]{_serverName}接收异常:{ex.Message}",
                    _serviceName,
                    ex.Message
                );
            }
            finally
            {
                client.Close();
                client.Dispose();
                ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieClearanceScan", false);
                Serilog.Log.Warning("[扫描枪数据推送]{_serviceName}:客户端Socket已释放");
            }
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var re = _mcp.Read(_clearancResultPlcInfo);
            ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieClearance", re.IsSuccess);
            if (re.IsSuccess == false)
            {
                return Result.Fail("PLC通讯失败");
            }
            _clearance1ReTmp = DataConvertExpand.BoolArrayToByte(
                new bool[]
                {
                    _clearance1OK.Value[0].ObjToBool(),
                    _clearance1NG.Value[0].ObjToBool(),
                }
            );

            _clearance2ReTmp = DataConvertExpand.BoolArrayToByte(
                new bool[]
                {
                    _clearance2OK.Value[0].ObjToBool(),
                    _clearance2NG.Value[0].ObjToBool(),
                }
            );
            if (_clearance1Re != _clearance1ReTmp || _clearance2Re != _clearance2ReTmp)
            {
                return Result.Success();
            }
            else
            {
                return Result.Fail("PLC未触发采集信号");
            }
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            Tb_Factory4Workshop4_10Line_Clearance dataValue = null;
            if (_clearance1ReTmp != _clearance1Re && _clearance1ReTmp != 0)
            {
                var revalue = _mcp.Read(_clearance1PlcInfo);

                if (revalue.IsSuccess)
                {
                    var sn = _clearance1SN.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false && _lastClearance1SN != sn)
                    {
                        var clearanceInfo = await _clearanceRepository.QueryableFirstAsync(
                            x => x.SN == sn,
                            x => x.Clearance1Time
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_Clearance()
                        {
                            Clearance1Result =
                                _clearance1ReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            SN = sn,
                            PositiveGap = _positiveGapPlcInfo.Value[0].ToString(),
                            LowerLoad = _lowerLoadPlcInfo.Value[0].ToString(),
                            UpperLoad = _uperLoadPlcInfo.Value[0].ToString(),
                            Clearance1Time = DateTime.Now,
                        };

                        if (clearanceInfo != null)
                        {
                            await _clearanceRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new
                                {
                                    x.Clearance1Result,
                                    x.PositiveGap,
                                    x.LowerLoad,
                                    x.UpperLoad,
                                    x.Clearance1Time,
                                }
                            );
                            await _summaryRepository.UpDataAsync(
                                dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                                x => new { x.SN },
                                x => new
                                {
                                    x.Clearance1Result,
                                    x.PositiveGap,
                                    x.LowerLoad,
                                    x.UpperLoad,
                                    x.Clearance1Time,
                                }
                            );
                        }
                        else
                        {
                            await _clearanceRepository.InsertableAsync(dataValue);
                            if (
                                await _summaryRepository.QueryableFirstAsync(x =>
                                    x.SN == dataValue.SN
                                ) == null
                            )
                            {
                                await _summaryRepository.InsertableAsync(
                                    dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>()
                                );
                            }
                        }
                        _lastClearance1SN = sn;
                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "ClearanceStation1Data",
                            new Factory4Workshop4_10Line_Clearance_Station1Dto
                            {
                                SN = dataValue.SN,
                                Clearance1Result = dataValue.Clearance1Result,
                                PositiveGap = dataValue.PositiveGap,
                                LowerLoad = dataValue.LowerLoad,
                                UpperLoad = dataValue.UpperLoad,
                                Clearance1Time = dataValue.Clearance1Time,
                            }
                        );
                    }
                }
            }
            if (_clearance2Re != _clearance2ReTmp && _clearance2ReTmp != 0)
            {
                var reSN = _mcp.Read(_clearance2PlcInfo);
                if (reSN.IsSuccess)
                {
                    var sn = _clearance2SN.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false && _lastClearance2SN != sn)
                    {
                        var clearanceInfo = await _clearanceRepository.QueryableFirstAsync(
                            x => x.SN == sn,
                            x => x.Clearance2Time
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_Clearance()
                        {
                            Clearance2Result =
                                _clearance2ReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            SN = sn,
                            Offset = _offsetPlcInfo.Value[0].ToString(),
                            BeforePressIn = _beforePressInPlcInfo.Value[0].ToString(),
                            AfterPressIn = _afterPressInPlcInfo.Value[0].ToString(),
                            Gap = _gapPlcInfo.Value[0].ToString(),
                            Clearance2Time = DateTime.Now,
                        };

                        if (clearanceInfo != null)
                        {
                            await _clearanceRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new
                                {
                                    x.Clearance2Result,
                                    x.Offset,
                                    x.BeforePressIn,
                                    x.AfterPressIn,
                                    x.Gap,
                                    x.Clearance2Time,
                                }
                            );
                        }
                        else
                        {
                            await _clearanceRepository.InsertableAsync(dataValue);
                        }

                        await _summaryRepository.UpDataAsync(
                            dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                            x => x.SN,
                            x => new
                            {
                                x.Clearance2Result,
                                x.Offset,
                                x.BeforePressIn,
                                x.AfterPressIn,
                                x.Gap,
                                x.Clearance2Time,
                            }
                        );
                        _lastClearance2SN = sn;
                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "ClearanceStation2Data",
                            new Factory4Workshop4_10Line_Clearance_Station2Dto
                            {
                                SN = dataValue.SN,
                                Clearance2Result = dataValue.Clearance2Result,
                                Offset = dataValue.Offset,
                                BeforePressIn = dataValue.BeforePressIn,
                                AfterPressIn = dataValue.AfterPressIn,
                                Gap = dataValue.Gap,
                                Clearance2Time = dataValue.Clearance2Time,
                            }
                        );
                    }
                }
            }

            return Result<object>.Success(dataValue);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            _clearance1Re = _clearance1ReTmp;
            _clearance2Re = _clearance2ReTmp;
        }
    }
}
