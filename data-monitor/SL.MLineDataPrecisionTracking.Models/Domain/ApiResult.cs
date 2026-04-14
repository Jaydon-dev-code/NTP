using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class ApiResult<T>
    {
        public int Code { get; set; }

        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
    }
    public class ApiResult
    {
        public int Code { get; set; }

        public bool IsSuccess { get; set; }
        public string Msg { get; set; }

        public static ApiResult Bad(string erorr)
        {
            return new ApiResult() { IsSuccess=false, Msg= erorr };
        }
        public static ApiResult OK()
        {
            return new ApiResult() { IsSuccess = true, Msg = "成功" };
        }
    }

}
