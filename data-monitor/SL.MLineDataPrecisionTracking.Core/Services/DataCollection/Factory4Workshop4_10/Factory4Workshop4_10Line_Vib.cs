using Mapster;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_Vib : Factory4Workshop4_10LineBase
    {
        Socket _serverSocket;
        protected DevPlcPointDto _issueSancInfoPoint;
        protected override string _serviceName => "四分厂4-10-震动";

        protected override Type _dataModelType { get; set; } =
            typeof(Tb_Factory4Workshop4_10Line_Vib);
        Tb_Factory4Workshop4_10Line_VibRepository _factory4Workshop;

        public Factory4Workshop4_10Line_Vib(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            Tb_Factory4Workshop4_10Line_VibRepository factory4Workshop,
            IHubContext chatHub
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _factory4Workshop = factory4Workshop;
        }

        protected override async Task<Result> HandshakeAsync()
        {
            _chatHub.Clients.All.IsOnlieVib = true;
            return Result.Success();
        }

        protected override Result IntiSetting()
        {
            _issueSancInfoPoint = _linePlcInfo.First(x => x.PointName == "下发序列码");
            _issueSancInfoPoint.Value = new List<object>() { "" };
            _linePlcInfo.Remove(_issueSancInfoPoint);
            ScanSocket();
            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            var readValue = _mcp.Read(_linePlcInfo);
            if (readValue.IsSuccess is false)
            {
                return Result<object>.Fail(readValue.Message);
            }

            return Expand.SugarColumnReflectAssign(readValue, _dataModelType);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            Tb_Factory4Workshop4_10Line_Vib data = (Tb_Factory4Workshop4_10Line_Vib)interact.Data;
            _chatHub.Clients.All.Factory4Workshop4_10Line_VibDto =
             data.Adapt<Factory4Workshop4_10Line_VibDto>();

            if (string.IsNullOrEmpty(data.SN))
            {
                return;
            }

            await _factory4Workshop.InsertableAsync(data);

            await _summaryRepository.UpDataAsync(
                data.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                x => x.SN,
                _upCloName
            );
         
        }
        /// <summary>
        /// 扫码枪socket通讯，公开用于单元测试
        /// </summary>
        public  void ScanSocket()
        {
            if (
                int.TryParse(
                    ConfigurationManager.AppSettings["Factory4_10ScanPort"],
                    out int factory4_10ScanPort
                ) == false
            )
            {
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
            while (true)
            {
                Socket client = _serverSocket.Accept();

                // 新开线程持续接收条码
                new Thread(() => ReceiveBarcodeLoop(client)).Start();
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
                    _issueSancInfoPoint.Value[0] = ccanInfo;
                    _mcp.Write(_issueSancInfoPoint);
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
                Serilog.Log.Warning("[扫描枪数据推送]{_serverName}:客户端Socket已释放");
            }
        }
    }
}
