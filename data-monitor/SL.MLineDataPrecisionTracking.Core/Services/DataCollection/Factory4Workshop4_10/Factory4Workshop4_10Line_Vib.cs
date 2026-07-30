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
using NPOI.XWPF.UserModel;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
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
    public class Factory4Workshop4_10Line_Vib : Factory4Workshop4_10LineBase
    {
        Socket _serverSocket;

        /// <summary>
        /// 下发序列码给plc的点位
        /// </summary>
        protected DevPlcPointDto _issueSancInfoPoint;
        protected override string _serviceName => "四分厂4-10-震动";

        Tb_Factory4Workshop4_10Line_VibRepository _vibRepository;
        Mc1ECommunication _mc1ECommunication;

        DevPlcPointDto _vibOK;
        DevPlcPointDto _vibNG;
        DevPlcPointDto _vibSN;
        List<DevPlcPointDto> _vibResultPlcInfo;

        byte _vibRe;
        byte _vibReTmp;

        public Factory4Workshop4_10Line_Vib(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            Tb_Factory4Workshop4_10Line_VibRepository factory4Workshop,
            IHubContext chatHub,
            Mc1ECommunication mc1ECommunication
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _mc1ECommunication = mc1ECommunication;
            _vibRepository = factory4Workshop;
        }

        protected override async Task<Result> IntiSetting()
        {
            _issueSancInfoPoint = _linePlcInfo.First(x => x.PointName == "下发震动SN");
            _issueSancInfoPoint.Value = new List<object>() { "" };
            _vibOK = _linePlcInfo.First(x => x.PointName == "震动OK");
            _vibNG = _linePlcInfo.First(x => x.PointName == "震动NG");
            _vibSN = _linePlcInfo.First(x => x.PointName == "震动SN");
            _vibResultPlcInfo = new List<DevPlcPointDto>() { _vibOK, _vibNG };
            Task.Run(() => ScanSocket());

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var re = _mc1ECommunication.Read(_vibResultPlcInfo);
            ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieVib", re.IsSuccess);
            if (re.IsSuccess == false)
            {
                return Result.Fail("PLC通讯失败");
            }
            _vibReTmp = Expand.BoolArrayToByte(
                new bool[] { _vibOK.Value[0].ObjToBool(), _vibNG.Value[0].ObjToBool() }
            );
            if (_vibReTmp != _vibRe)
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
            Tb_Factory4Workshop4_10Line_Vib dataValue = null;
            if (_vibReTmp == 0)
            {
                return Result<object>.Success(null);
            }

            var revalue = _mc1ECommunication.Read(_vibSN);

            if (revalue.IsSuccess)
            {
                var sn = _vibSN.Value[0].ToString();
                if (string.IsNullOrEmpty(sn) == false)
                {
                    var clearanceInfo = await _vibRepository.QueryableFirstAsync(x => x.SN == sn);
                    dataValue = new Tb_Factory4Workshop4_10Line_Vib()
                    {
                        VibCrackResult = _vibReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                        SN = sn,
                        VibCrackTime = DateTime.Now,
                    };

                    if (clearanceInfo != null)
                    {
                        await _vibRepository.UpDataAsync(
                            dataValue,
                            x => new { x.SN },
                            x => new { x.VibCrackResult, x.VibCrackTime }
                        );
                    }
                    else
                    {
                        await _vibRepository.InsertableAsync(dataValue);
                    }
                    await _summaryRepository.UpDataAsync(
                        dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                        x => new { x.SN },
                        x => new { x.VibCrackResult, x.VibCrackTime }
                    );

                    ((IClientProxy)_chatHub.Clients.All).Invoke(
                        "VibData",
                        new Factory4Workshop4_10Line_VibDto
                        {
                            SN = dataValue.SN,
                            VibCrackResult = dataValue.VibCrackResult,
                            VibCrackTime = dataValue.VibCrackTime,
                        }
                    );
                }
            }
            return Result<object>.Success(dataValue);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            _vibRe = _vibReTmp;
        }

        public override void Stop()
        {
            if (_serverSocket != null)
            {
                try
                {
                    _serverSocket.Close();
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(
                        "[扫描枪数据推送]{_serviceName}关闭Socket异常:{ex.Message}",
                        _serviceName,
                        ex.Message
                    );
                }
                finally
                {
                    _serverSocket = null;
                }
            }
            base.Stop();
        }

        /// <summary>
        /// 扫码枪socket通讯，公开用于单元测试
        /// </summary>
        public void ScanSocket()
        {
            if (
                int.TryParse(
                    ConfigurationManager.AppSettings["Factory4_10ScanPort"],
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
                    ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieVibScan", true);

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
                    var ccanInfo = soureByte.BytesToAscii(soureLen);
                    //if (!Expand.IsRunningInMSTest())
                    //{
                    _issueSancInfoPoint.Value[0] = ccanInfo;
                    _mc1ECommunication.Write(_issueSancInfoPoint);
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
                ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieVibScan", false);
                Serilog.Log.Warning("[扫描枪数据推送]{_serviceName}:客户端Socket已释放");
            }
        }
    }
}
