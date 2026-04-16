using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class ProductionLineDataCollectionService
    {
        Tb_EquipmentRepository _equipmentRepository;

        List<DevPlcPointMcDto> _lineReadPlcInfo;
        McpCommunication _mcpCommunication;

        int _plcJump;
        DevPlcPointMcDto _plcJumpPoint;
        DevPlcPointMcDto _pcJumpPoint;
        int _pcJump;

        public ProductionLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepository,
            McpCommunication mcpCommunication
        )
        {
            _equipmentRepository = equipmentRepository;
            _mcpCommunication = mcpCommunication;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await InitPlcAddre();

                    while (true)
                    {
                        try
                        {
                            if (await CanCollection() is false)
                            {
                                continue;
                            }
                            CollectionData();
                            CallPlcCollectionOK();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void CollectionData() { }

        private async Task CallPlcCollectionOK()
        { // 将读取的plc的 跳变回写
            _pcJumpPoint.Value = new List<object> { _pcJump };
            if ((await _mcpCommunication.WriteAsync(_pcJumpPoint)).IsSuccess)
            { //写成功后更新自己的的跳变
                _plcJump = _plcJump;
            }
        }

        private async Task<bool> CanCollection()
        {
            var plcJumpValue = await _mcpCommunication.ReadAsync(_plcJumpPoint);
            if (plcJumpValue.IsSuccess is false)
            {
                return false;
            }
            var readPlcJump = plcJumpValue.Data.Value[0].ObjToInt();
            if (readPlcJump == 0 || readPlcJump == _plcJump)
            {
                return false;
            }
            _plcJump = readPlcJump;
            return true;
        }

        private async Task InitPlcAddre()
        {
            _lineReadPlcInfo = new List<DevPlcPointMcDto>();
            foreach (var devInfo in await _equipmentRepository.GetEquipmentAllAsync())
            {
                foreach (var plcLinkeInfo in devInfo.PlcConnections)
                {
                    foreach (var plcAddres in plcLinkeInfo.Points)
                    {
                        _lineReadPlcInfo.Add(
                            new DevPlcPointMcDto(
                                devInfo.DeviceName,
                                plcAddres.PointName,
                                plcLinkeInfo.IpAddress,
                                plcLinkeInfo.Port,
                                plcAddres.Area.ToPrefix(),
                                plcAddres.DataType.ToTypeCode(),
                                plcAddres.Address,
                                plcAddres.Length
                            )
                        );
                    }
                }
            }
        }
    }
}
