using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class RefinedSearchCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string APalletNo { get; set; }
        public string BPalletNo { get; set; }
        public string ANGNo { get; set; }
        public string BNGNo { get; set; }
        public string ModelName { get; set; }
        public string Op { get; set; }
        public string Result { get; set; }
        public string MarkingNo { get; set; }
        public bool IsScanCode { get; set; }
    }
}
