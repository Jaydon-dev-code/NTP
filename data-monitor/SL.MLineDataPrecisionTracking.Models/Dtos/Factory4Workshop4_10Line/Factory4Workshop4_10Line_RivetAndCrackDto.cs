using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_RivetAndCrackDto
    {
        public DateTime RecordTime
        {
            get; set;
        }
        public string SN
        {
            get; set;
        }
        public ResultEnum RivetAndCrackResult
        {
            get; set;
        }
    }
}
