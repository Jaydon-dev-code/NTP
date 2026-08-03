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
        DevPlcPointDto _RivetingInspectionFormingHeightPlcInfo;
        DevPlcPointDto _rivetingInspection2HeightPlcInfo;
        List<DevPlcPointDto> _rivetingInspectionPlcInfo;
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
            _rivetingInspection1HeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "铆接检测1高度"
            );
            _RivetingInspectionFormingHeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "铆接成型高度"
            );
            _rivetingInspection2HeightPlcInfo = _linePlcInfo.First(x =>
                x.PointName == "铆接检测2高度"
            );
            _rivetAndCrackResultPlcInfo = new()
            {
                _rivetingOK,
                _rivetingNG,
                _spinRivetingOK,
                _spinRivetingNG,
            };
            _rivetingSN = _linePlcInfo.First(x => x.PointName == "铆接SN");

            _spinRivetingSNPlcInfo = _linePlcInfo.First(x => x.PointName == "旋铆检测SN");
            _rivetingInspectionPlcInfo = new List<DevPlcPointDto>()
            {
                _rivetingInspection1HeightPlcInfo,
                _RivetingInspectionFormingHeightPlcInfo,
                _rivetingInspection2HeightPlcInfo,
                _spinRivetingSNPlcInfo,
                _rivetingSN,
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
            Tb_Factory4Workshop4_10Line_RivetAndCrack dataValue = null;
            if (_rivetingReTmp != _rivetingRe && _rivetingReTmp != 0)
            {
                var revalue = _mcp.Read(_rivetingInspectionPlcInfo);
                if (revalue.IsSuccess)
                {
                    var sn = _rivetingSN.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false)
                    {
                        var clearanceInfo = await _rivetAndCrackRepository.QueryableFirstAsync(x =>
                            x.SN == _rivetingSN.Value[0].ToString()
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_RivetAndCrack()
                        {
                            RivetingResult = _rivetingReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            RivetingInspection1Height = _rivetingInspection1HeightPlcInfo
                                .Value[0]
                                .ToString(),
                            RivetingInspectionFormingHeight =
                                _RivetingInspectionFormingHeightPlcInfo.Value[0].ToString(),
                            RivetingInspection2Height = _rivetingInspection2HeightPlcInfo
                                .Value[0]
                                .ToString(),
                            SN = sn,
                            RivetingTime = DateTime.Now,
                        };

                        if (clearanceInfo != null)
                        {
                            await _rivetAndCrackRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new
                                {
                                    x.RivetingResult,
                                    x.RivetingTime,
                                    x.RivetingInspection1Height,
                                    x.RivetingInspectionFormingHeight,
                                    x.RivetingInspection2Height,
                                }
                            );
                        }
                        else
                        {
                            await _rivetAndCrackRepository.InsertableAsync(dataValue);
                        }
                        await _summaryRepository.UpDataAsync(
                            dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                            x => new { x.SN },
                            x => new
                            {
                                x.RivetingResult,
                                x.RivetingTime,
                                x.RivetingInspection1Height,
                                x.RivetingInspectionFormingHeight,
                                x.RivetingInspection2Height,
                            }
                        );

                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "RivetingData",
                            new Factory4Workshop4_10Line_RivetAndCrack_RivetingDto
                            {
                                SN = dataValue.SN,
                                RivetingResult = dataValue.RivetingResult,
                                RivetingInspection1Height = dataValue.RivetingInspection1Height,
                                RivetingInspectionFormingHeight =
                                    dataValue.RivetingInspectionFormingHeight,
                                RivetingInspection2Height = dataValue.RivetingInspection2Height,
                                RivetingTime = dataValue.RivetingTime,
                            }
                        );
                    }
                }
            }

            if (_spinRivetingRe != _spinRivetingReTmp && _spinRivetingReTmp != 0)
            {
                var revalue = _mcp.Read(_spinRivetingSNPlcInfo);
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

                            SN = sn,
                            SpinRivetingTime = DateTime.Now,
                        };

                        if (spinInfo != null)
                        {
                            await _rivetAndCrackRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new { x.SpinRivetingTime, x.SpinRivetingResult }
                            );
                        }
                        else
                        {
                            await _rivetAndCrackRepository.InsertableAsync(dataValue);
                        }

                        await _summaryRepository.UpDataAsync(
                            dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                            x => x.SN,
                            x => new { x.SpinRivetingResult, x.SpinRivetingTime }
                        );

                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "SpinRivetingData",
                            new Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto
                            {
                                SN = dataValue.SN,
                                SpinRivetingResult = dataValue.SpinRivetingResult,
                                SpinRivetingTime = dataValue.SpinRivetingTime,
                            }
                        );
                    }
                }
            }
            return Result<object>.Success(dataValue);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            _rivetingRe = _rivetingReTmp;
            _spinRivetingRe = _spinRivetingReTmp;
        }
    }
}
