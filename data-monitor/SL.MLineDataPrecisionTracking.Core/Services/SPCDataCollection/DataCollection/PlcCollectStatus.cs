using System;
using System.Collections.Generic;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection
{
    /// <summary>
    /// PLC 状态采集 — 从 PLC 读取多个点位的测量结果。
    /// init 入参 objs[1] 为 List&lt;DevPlcPointDto&gt;（Manager 统一解析后传入）。
    /// </summary>
    public class PlcCollectStatus : SPCDataCollectionAbstract
    {
        McpCommunication _mcp;
        List<DevPlcPointDto> _reValuePoints = new List<DevPlcPointDto>();
        int _lastRe;

        public PlcCollectStatus(McpCommunication mcp)
        {
            _mcp = mcp;
        }

        public override string _featureName { get; set; }
        public override SPCDataSource DataSource => SPCDataSource.PLC;
        public override string Description => "PLC 状态采集";

        public override void Execute()
        {
            if (_reValuePoints == null || _reValuePoints.Count == 0) return;

            foreach (var point in _reValuePoints)
            {
                var re = _mcp.Read(point);
                if (re.IsSuccess)
                {
                    var tmpRe = re.Data.Value[0].ObjToInt();
                    if (_lastRe == 0 && tmpRe != 0) { /* 上升沿触发 */ }
                    _lastRe = tmpRe;
                }
            }
        }

        /// <summary>
        /// 初始化：objs[0] = 特性名称(string)，objs[1] = List&lt;DevPlcPointDto&gt;（已解析点位列表）。
        /// </summary>
        protected override void InitCollectionInfo(object[] objs)
        {
            _reValuePoints.Clear();

            if (objs.Length > 1 && objs[1] is List<DevPlcPointDto> points)
            {
                _reValuePoints.AddRange(points);
            }
        }
    }
}
