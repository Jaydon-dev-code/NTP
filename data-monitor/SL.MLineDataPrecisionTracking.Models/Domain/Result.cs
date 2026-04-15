using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    /// <summary>
    /// 【程序内部使用】通用操作结果
    /// </summary>
    public class Result
    {
        // 是否成功
        public bool IsSuccess { get; set; }

        // 提示消息/错误消息
        public string Message { get; set; }

        #region 静态快速创建
        public static Result Success() => new Result { IsSuccess = true };
        public static Result Success(string message) => new Result { IsSuccess = true, Message = message };

        public static Result Fail(string message) => new Result { IsSuccess = false, Message = message };
        #endregion
    }

    /// <summary>
    /// 【程序内部使用】带返回数据的通用结果
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Result<T> : Result
    {
        // 业务数据
        public T Data { get; set; }

        #region 静态快速创建
        public static Result<T> Success(T data) => new Result<T> { IsSuccess = true, Data = data };
        public static Result<T> Success(T data, string message) => new Result<T> { IsSuccess = true, Data = data, Message = message };

        public new static Result<T> Fail(string message) => new Result<T> { IsSuccess = false, Message = message };
        #endregion
    }
}
