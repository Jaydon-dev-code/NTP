using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class CollectionInfo
    {
        public CollectionInfo(List<DevPlcPointDto> devPlcs, List<DevPlcPointDto> alarm)
        {
            _statusPoint = devPlcs.FirstOrDefault(x => x.PointName == "设备状态");
            _beatPoint = devPlcs.FirstOrDefault(x => x.PointName == "生产节拍");
            _producedTotalPoint = devPlcs.FirstOrDefault(x => x.PointName == "生产总数");
            _producedOKPoint = devPlcs.FirstOrDefault(x => x.PointName == "生产OK数");
            _producedNGPoint = devPlcs.FirstOrDefault(x => x.PointName == "生产NG数");
            _warValuePoint = alarm;
        }

        public int Status { get; set; }
        DevPlcPointDto _statusPoint { get; set; }
        public float Beat { get; set; }
        DevPlcPointDto _beatPoint { get; set; }
        public int ProducedTotal { get; set; }
        DevPlcPointDto _producedTotalPoint { get; set; }
        public int ProducedOK { get; set; }

        DevPlcPointDto _producedOKPoint { get; set; }
        public int ProducedNG { get; set; }
        DevPlcPointDto _producedNGPoint { get; set; }
        public bool[] WarValue { get; set; }
        List<DevPlcPointDto> _warValuePoint { get; set; }

        public string[] WarInfo { get; set; }

        public bool IsChang()
        {
            var tmpStatus = _statusPoint.Value[0].ObjToInt();
            var tmpBeat =
                _beatPoint.Value[0] == null ? 0 : float.Parse(_beatPoint.Value[0].ToString());
            var tmpProducedTotal = _producedTotalPoint.Value[0].ObjToInt();
            var tmpProducedOK = _producedOKPoint.Value[0].ObjToInt();
            var tmpProducedNG = _producedNGPoint.Value[0].ObjToInt();
            var tmpWarValue = _warValuePoint.Select(x => x.Value[0].ObjToBool()).ToArray();
            var re =
                this.Status == _statusPoint.Value[0].ObjToInt()
                && this.Beat == tmpBeat
                && this.ProducedTotal == tmpProducedTotal
                && this.ProducedOK == tmpProducedOK
                && this.ProducedNG == tmpProducedNG
                && this.WarValue.SequenceEqual(tmpWarValue);
            if (re)
            {
                this.Status = tmpStatus;
                this.Beat = tmpBeat;
                this.ProducedTotal = tmpProducedTotal;
                this.ProducedOK = tmpProducedOK;
                this.ProducedNG = tmpProducedNG;
                this.WarValue = tmpWarValue;
            }

            return re;
        }

    }
}
