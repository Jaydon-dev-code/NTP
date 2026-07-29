using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using NPOI.POIFS.Crypt.Dsig;
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
    public class Factory4Workshop4_10Line_RivetAndCrack : Factory4Workshop4_10LineBase
    {
        protected override string _serviceName => "四分厂4-10-铆接和裂纹";
        Tb_Factory4Workshop4_10Line_RivetAndCrackRepository _rivetAndCrackRepository;

        DevPlcPointDto _rivetingOK;
        DevPlcPointDto _rivetingNG;
        DevPlcPointDto _spinRivetingOK;
        DevPlcPointDto _spinRivetingNG;
        List<DevPlcPointDto> _rivetAndCrackResultPlcInfo;
        DevPlcPointDto _rivetingSN;
        DevPlcPointDto _rivetingInspection1HeightPlcInfo;
        DevPlcPointDto _spiralRivetingFormingHeightPlcInfo;
        DevPlcPointDto _rivetingInspection2HeightPlcInfo;
        List<DevPlcPointDto> _spinRivetingPlcInfo;
        DevPlcPointDto _spinRivetingSNPlcInfo;
        byte _rivetingRe;
        byte _rivetingReTmp;
        byte _spinRivetingRe;
        byte _spinRivetingReTmp;

        public Factory4Workshop4_10Line_RivetAndCrack(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            Tb_Factory4Workshop4_10Line_RivetAndCrackRepository rivetAndCrackRepository,
            IHubContext chatHub
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _rivetAndCrackRepository = rivetAndCrackRepository;
        }

        protected override async Task<Result> IntiSetting()
        {
            _rivetingOK = _linePlcInfo.First(x => x.PointName == "铆接OK");
            _rivetingNG = _linePlcInfo.First(x => x.PointName == "铆接NG");

            _spinRivetingOK = _linePlcInfo.First(x => x.PointName == "旋铆检测OK");
            _spinRivetingNG = _linePlcInfo.First(x => x.PointName == "旋铆检测NG");
            _rivetAndCrackResultPlcInfo = new()
            {
                _rivetingOK,
                _rivetingNG,
                _spinRivetingOK,
                _spinRivetingNG,
            };
            _rivetingSN = _linePlcInfo.First(x => x.PointName == "铆接SN");
            _rivetingInspection1HeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "旋铆检测1高度"
            );
            _spiralRivetingFormingHeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "旋铆成型高度"
            );
            _rivetingInspection2HeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "旋铆检测2高度"
            );
            _spinRivetingSNPlcInfo = _linePlcInfo.First(x => x.PointName == "旋铆检测SN");
            _spinRivetingPlcInfo = new List<DevPlcPointDto>()
            {
                _rivetingInspection1HeightPlcInfo,
                _spiralRivetingFormingHeightPlcInfo,
                _rivetingInspection2HeightPlcInfo,
                _spinRivetingSNPlcInfo,
            };
            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var re = _mcp.Read(_rivetAndCrackResultPlcInfo);
            ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieRivetAndCrack", re.IsSuccess);
            if (re.IsSuccess == false)
            {
                return Result.Fail("PLC通讯失败");
            }
            _rivetingReTmp = Expand.BoolArrayToByte(
                new bool[] { _rivetingOK.Value[0].ObjToBool(), _rivetingNG.Value[0].ObjToBool() }
            );

            _spinRivetingReTmp = Expand.BoolArrayToByte(
                new bool[]
                {
                    _spinRivetingOK.Value[0].ObjToBool(),
                    _spinRivetingNG.Value[0].ObjToBool(),
                }
            );
            if (_rivetingRe != _rivetingReTmp || _spinRivetingRe != _spinRivetingReTmp)
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
            Tb_Factory4Workshop4_10Line_RivetAndCrack dataValue;
            if (_rivetingReTmp != _rivetingRe && _rivetingReTmp != 0)
            {
                var revalue = _mcp.Read(_rivetingSN);
                if (revalue.IsSuccess)
                {
                    var sn = _rivetingSN.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false) { }

                    var clearanceInfo = await _rivetAndCrackRepository.QueryableFirstAsync(x =>
                        x.SN == _rivetingSN.Value[0].ToString()
                    );
                    dataValue = new Tb_Factory4Workshop4_10Line_RivetAndCrack()
                    {
                        RivetingResult = _rivetingReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                        SN = sn,
                        RivetingTime = DateTime.Now,
                    };

                    if (clearanceInfo != null)
                    {
                        await _rivetAndCrackRepository.UpDataAsync(
                            dataValue,
                            x => new { x.SN },
                            x => new { x.RivetingResult, x.RivetingTime }
                        );
                    }
                    else
                    {
                        await _rivetAndCrackRepository.InsertableAsync(dataValue);
                    }
                    await _summaryRepository.UpDataAsync(
                        dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                        x => new { x.SN },
                        x => new { x.RivetingResult, x.RivetingTime }
                    );

                    ((IClientProxy)_chatHub.Clients.All).Invoke(
                        "RivetingData",
                        new Factory4Workshop4_10Line_RivetAndCrack_RivetingDto
                        {
                            SN = dataValue.SN,
                            RivetingResult = dataValue.RivetingResult,
                            RivetingTime = dataValue.RivetingTime,
                        }
                    );
                }
            }
            if (_spinRivetingRe != _spinRivetingReTmp && _spinRivetingReTmp != 0)
            {
                var revalue = _mcp.Read(_spinRivetingPlcInfo);
                if (revalue.IsSuccess)
                {
                    var sn = _spinRivetingSNPlcInfo.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false)
                    {
                        var spinInfo = await _rivetAndCrackRepository.QueryableFirstAsync(x =>
                            x.SN == sn
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_RivetAndCrack()
                        {
                            SpinRivetingResult =
                                _spinRivetingReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            RivetingInspection1Height = _rivetingInspection1HeightPlcInfo
                                .Value[0]
                                .ToString(),
                            SpiralRivetingFormingHeight = _spiralRivetingFormingHeightPlcInfo
                                .Value[0]
                                .ToString(),
                            RivetingInspection2Height = _rivetingInspection2HeightPlcInfo
                                .Value[0]
                                .ToString(),
                            SN = sn,
                            SpinRivetingTime = DateTime.Now,
                        };

                        if (spinInfo != null)
                        {
                            await _rivetAndCrackRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new
                                {
                                    x.RivetingInspection1Height,
                                    x.SpiralRivetingFormingHeight,
                                    x.RivetingInspection2Height,
                                    x.SpinRivetingTime,
                                    x.SpinRivetingResult,
                                }
                            );
                        }
                        else
                        {
                            await _rivetAndCrackRepository.InsertableAsync(dataValue);
                        }

                        await _summaryRepository.UpDataAsync(
                            dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                            x => x.SN,
                            x => new
                            {
                                x.SpinRivetingResult,
                                x.RivetingInspection1Height,
                                x.SpiralRivetingFormingHeight,
                                x.RivetingInspection2Height,
                                x.SpinRivetingTime,
                            }
                        );

                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "SpinRivetingData",
                            new Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto
                            {
                                SN = dataValue.SN,
                                RivetingInspection1Height = dataValue.RivetingInspection1Height,
                                SpiralRivetingFormingHeight = dataValue.SpiralRivetingFormingHeight,
                                RivetingInspection2Height = dataValue.RivetingInspection2Height,
                                SpinRivetingTime = dataValue.SpinRivetingTime,
                            }
                        );
                    }
                }
            }
            return Result<object>.Success(null);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            _rivetingRe = _rivetingReTmp;
            _spinRivetingRe = _spinRivetingReTmp;
        }
    }
}
