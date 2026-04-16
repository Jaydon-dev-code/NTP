using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public abstract class ProLineDataCollectionServiceAbstract
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
                        if (await CollectionData())
                        {
                            await CallPlcCollectionOK();
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
            catch (Exception ex)
            {
                throw;
            }
        }

        protected abstract Task<bool> CollectionData();

        protected virtual async Task CallPlcCollectionOK()
        {
            _pcCallPlcCollctionOk.Value = new List<object>() { 0 };
            await _mcp.WriteAsync(_pcCallPlcCollctionOk);
        }

        protected virtual async Task<bool> CanCollection()
        {
            var re = await _mcp.ReadAsync(_plcCallPCCanCollectionPoint);
            if (re.IsSuccess is false || re.Data.Value[0].ObjToInt() == 0)
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
