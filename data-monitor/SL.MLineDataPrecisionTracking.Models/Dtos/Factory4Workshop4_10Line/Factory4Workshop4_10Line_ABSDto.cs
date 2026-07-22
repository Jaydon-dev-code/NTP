using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_ABSDto
    {
        public DateTime RecordTime { get; set; }
        public string SN { get; set; }
        public ResultEnum ABSResult { get; set; }
    }
}
