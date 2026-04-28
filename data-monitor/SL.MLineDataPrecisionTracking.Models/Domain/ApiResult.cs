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
        public string Message { get; set; }
        public T Data { get; set; }

        #region 静态快速创建
        public static ApiResult<T> Success(T data) =>
            new ApiResult<T> { IsSuccess = true, Data = data };

        public static ApiResult<T> Success(T data, string message) =>
            new ApiResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
            };

        public static new ApiResult<T> Fail(string message) =>
            new ApiResult<T> { IsSuccess = false, Message = message };
        #endregion
    }

    public class ApiResult
    {
        public int Code { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        #region 静态快速创建
        public static ApiResult Success() => new ApiResult { IsSuccess = true, Message = "成功" };

        public static ApiResult Success(string message) =>
            new ApiResult { IsSuccess = true, Message = message };

        public static new ApiResult Fail(string message) =>
            new ApiResult { IsSuccess = false, Message = message };
        #endregion
    }
}
