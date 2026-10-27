using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    public class MesResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public MesResponseInfo Data { get; set; }
    }

    public class MesResponseInfo
    {
        public bool Success { get; set; }
    }
}
