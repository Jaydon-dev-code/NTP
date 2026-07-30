using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNet.SignalR;
using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Domain.Mc1E;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar.Extensions;
using IClientProxy = Microsoft.AspNet.SignalR.Hubs.IClientProxy;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10
{
    public class Factory4Workshop4_10Line_ABS : Factory4Workshop4_10LineBase
    {
        Tb_Factory4Workshop4_10Line_ABSRepository _aBSRepository;
        protected override string _serviceName => "四分厂4-10-ABS";

        DevPlcPointDto _aBSCheckOK;
        DevPlcPointDto _aBSCheckNG;
        DevPlcPointDto _aBSPressDownOK;
        DevPlcPointDto _aBSPressDownNG;
        DevPlcPointDto _aBSCheckSN;
        DevPlcPointDto _aBSPressDownSN;
        List<DevPlcPointDto> _absResultPlcInfo;
        byte _absCheckRe;
        byte _absCheckReTmp;
        byte _aBSPressDownRe;
        byte _aBSPressDownReTmp;

        public Factory4Workshop4_10Line_ABS(
            Tb_EquipmentRepository tb_EquipmentRepository,
            McpCommunication mcpCommunication,
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            IHubContext chatHub,
            Tb_Factory4Workshop4_10Line_ABSRepository aBSRepository
        )
            : base(tb_EquipmentRepository, mcpCommunication, summaryRepository, chatHub)
        {
            _aBSRepository = aBSRepository;
        }

        protected override async Task<Result> IntiSetting()
        {
            _aBSPressDownOK = _linePlcInfo.First(x => x.PointName == "ABS压紧OK");
            _aBSPressDownNG = _linePlcInfo.First(x => x.PointName == "ABS压紧NG");
            _aBSPressDownSN = _linePlcInfo.First(x => x.PointName == "ABS压紧SN");

            _aBSCheckOK = _linePlcInfo.First(x => x.PointName == "ABS检测OK");
            _aBSCheckNG = _linePlcInfo.First(x => x.PointName == "ABS检测NG");
            _aBSCheckSN = _linePlcInfo.First(x => x.PointName == "ABS检测SN");

            _absResultPlcInfo = new()
            {
                _aBSCheckOK,
                _aBSCheckNG,
                _aBSPressDownOK,
                _aBSPressDownNG,
            };
            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var re = _mcp.Read(_absResultPlcInfo);
            ((IClientProxy)_chatHub.Clients.All).Invoke("IsOnlieABS", re.IsSuccess);
            if (re.IsSuccess == false)
            {
                return Result.Fail("PLC通讯失败");
            }
            _aBSPressDownReTmp = Expand.BoolArrayToByte(
                new bool[]
                {
                    _aBSPressDownOK.Value[0].ObjToBool(),
                    _aBSPressDownNG.Value[0].ObjToBool(),
                }
            );

            _absCheckReTmp = Expand.BoolArrayToByte(
                new bool[] { _aBSCheckOK.Value[0].ObjToBool(), _aBSCheckNG.Value[0].ObjToBool() }
            );
            if (_aBSPressDownRe != _aBSPressDownReTmp || _absCheckRe != _absCheckReTmp)
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
            Tb_Factory4Workshop4_10Line_ABS dataValue = null;
            if (_aBSPressDownRe != _aBSPressDownReTmp && _aBSPressDownReTmp != 0)
            {
                var reSN = _mcp.Read(_aBSPressDownSN);

                if (reSN.IsSuccess)
                {
                    var sn = reSN.Data.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false)
                    {
                        var absInfo = await _aBSRepository.QueryableFirstAsync(
                            x => x.SN == sn,
                            x => x.ABSPressDownTime
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_ABS()
                        {
                            SN = sn,
                            ABSPressDownResult =
                                _aBSPressDownReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            ABSPressDownTime = DateTime.Now,
                        };
                        if (absInfo != null)
                        {
                            await _aBSRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new { x.ABSPressDownResult, x.ABSPressDownTime }
                            );
                        }
                        else
                        {
                            await _aBSRepository.InsertableAsync(dataValue);
                        }
                        await _summaryRepository.UpDataAsync(
                            dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                            x => new { x.SN },
                            x => new { x.ABSPressDownResult, x.ABSPressDownTime }
                        );

                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "ABSPressDownData",
                            new Factory4Workshop4_10Line_ABS_PressDownDto
                            {
                                SN = dataValue.SN,
                                ABSPressDownResult = dataValue.ABSPressDownResult,
                                ABSPressDownTime = dataValue.ABSPressDownTime,
                            }
                        );
                    }
                }
            }
            if (_absCheckReTmp != _absCheckRe && _absCheckReTmp != 0)
            {
                var reSN = _mcp.Read(_aBSCheckSN);

                if (reSN.IsSuccess)
                {
                    var sn = reSN.Data.Value[0].ToString();
                    if (string.IsNullOrEmpty(sn) == false)
                    {
                        var absInfo = await _aBSRepository.QueryableFirstAsync(
                            x => x.SN == sn,
                            x => x.ABSCheckTime
                        );
                        dataValue = new Tb_Factory4Workshop4_10Line_ABS()
                        {
                            SN = sn,
                            ABSCheckResult = _absCheckReTmp == 1 ? ResultEnum.OK : ResultEnum.NG,
                            ABSCheckTime = DateTime.Now,
                        };

                        if (absInfo != null)
                        {
                            await _aBSRepository.UpDataAsync(
                                dataValue,
                                x => new { x.SN },
                                x => new { x.ABSCheckResult, x.ABSCheckTime }
                            );
                            await _summaryRepository.UpDataAsync(
                                dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                                x => x.SN,
                                x => new { x.ABSCheckResult, x.ABSCheckTime }
                            );
                        }
                        else
                        {
                            await _aBSRepository.InsertableAsync(dataValue);
                            await _summaryRepository.UpDataAsync(
                                dataValue.Adapt<Tb_Factory4Workshop4_10LineSummary>(),
                                x => x.SN,
                                x => new
                                {
                                    x.ABSCheckResult,
                                    x.ABSCheckTime,
                                    x.RecordTime,
                                }
                            );
                        }

                        ((IClientProxy)_chatHub.Clients.All).Invoke(
                            "ABSCheckData",
                            new Factory4Workshop4_10Line_ABS_CheckDto
                            {
                                SN = dataValue.SN,
                                ABSCheckResult = dataValue.ABSCheckResult,
                                ABSCheckTime = dataValue.ABSCheckTime,
                            }
                        );
                    }
                }
            }

            return Result<object>.Success(dataValue);
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            _absCheckRe = _absCheckReTmp;
            _aBSPressDownRe = _aBSPressDownReTmp;
        }
    }
}
