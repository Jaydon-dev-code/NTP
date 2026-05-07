using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class RefinedSearchCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TrayNoA { get; set; }
        public string TrayNoB { get; set; }
        public string NgCodeA { get; set; }
        public string NgCodeB { get; set; }
        public string ModelName { get; set; }
        public string Op { get; set; }
        public ResultEnum Result { get; set; }
        public string MarkingNo { get; set; }
        public bool IsScanCode { get; set; }
    }
}
