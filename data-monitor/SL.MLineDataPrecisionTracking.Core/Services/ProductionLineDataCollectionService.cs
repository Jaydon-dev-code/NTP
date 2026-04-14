
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class ProductionLineDataCollectionService
    {
        Tb_EquipmentRepository _equipmentRepository;

        List<DevPlcPointMcDto> _lineReadPlcInfo;
        McpCommunication _mcpCommunication;
        public ProductionLineDataCollectionService(Tb_EquipmentRepository equipmentRepository, McpCommunication  mcpCommunication)
        {
            _equipmentRepository = equipmentRepository;
            _mcpCommunication= mcpCommunication;
        }

        public  async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await InitPlcAddre();
                  var readValue=   await _mcpCommunication.ReadAsync(_lineReadPlcInfo);
                   

                }
                catch (Exception ex)
                {

                    throw;
                }
                finally { await Task.Delay(1000); }
            

              
            }
        }

        private async Task InitPlcAddre()
        {
            _lineReadPlcInfo = new List<DevPlcPointMcDto>();
            foreach (var devInfo in await _equipmentRepository.GetEquipmentAllAsync())
            {
                //var devName = devInfo.DeviceName;
                foreach (var plcLinkeInfo in devInfo.PlcConnections)
                {
                    //var plcIp = plcLinkeInfo.IpAddress;
                    //var plcPort=plcLinkeInfo.Port;
                    foreach (var plcAddres in plcLinkeInfo.Points)
                    {
                        _lineReadPlcInfo.Add(new DevPlcPointMcDto(devInfo.DeviceName, plcAddres.PointName,plcLinkeInfo.IpAddress, plcLinkeInfo.Port,plcAddres.Area.ToPrefix(),plcAddres.DataType.ToTypeCode(),plcAddres.Address,plcAddres.Length));
                    }
                }
            }
         
        }
    }
}
