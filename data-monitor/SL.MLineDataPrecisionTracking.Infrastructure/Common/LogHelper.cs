using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Common
{
    /// <summary>
    /// 通用日志工具（全项目通用，不用注入）
    /// </summary>
    public static class LogHelper
    {
        // 全局静态 logger
        public static ILogger Logger { get; set; }




        //  Info
        public static void Info(string msg) => Logger?.Information(msg);
        public static void Info(string msg, params object[] args) => Logger?.Information(msg, args);

        //  Error
        public static void Error(string msg) => Logger?.Error(msg);
        public static void Error(Exception ex, string msg) => Logger?.Error(ex, msg);
        public static void Error(string msg, params object[] args) => Logger?.Error(msg, args);

        //  Debug
        public static void Debug(string msg) => Logger?.Debug(msg);

        //  Warn
        public static void Warn(string msg) => Logger?.Warning(msg);
    }
}