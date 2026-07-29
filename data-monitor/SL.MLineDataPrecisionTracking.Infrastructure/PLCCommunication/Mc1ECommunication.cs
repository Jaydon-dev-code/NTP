using McpXLib.Enums;
using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Domain.Mc1E;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Enum.Mc1E;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    /// <summary>
    /// MC QNA 1E 协议通信（二进制帧格式，基于 TCP）
    /// <para>
    /// 帧格式：
    ///   写请求 → 使用 Mc1EWirte 模型构建
    ///   读请求 → 手动构建二进制帧
    ///   响应   → 完成代码(1B) + 数据
    /// </para>
    /// <para>字节序：1E 二进制帧使用小端（Little-Endian），与主机一致，无需额外转换。</para>
    /// </summary>
    public class Mc1ECommunication : IPlcCommunication
    {
        /// <summary>连接池锁，保证 TcpClient 字典线程安全</summary>
        private readonly object _lockObj = new object();

        /// <summary>TCP 连接池，key = "ip:port"，复用长连接</summary>
        private readonly Dictionary<string, TcpClient> _tcpDic =
            new Dictionary<string, TcpClient>();

        /// <summary>TCP 收发超时（毫秒）</summary>
        private const int TimeoutMs = 3000;

        /// <summary>最大重试次数</summary>
        private const int MaxRetry = 3;

        /// <summary>重试间隔（毫秒）</summary>
        private const int RetryMs = 300;

        #region 读取

        private static string StripNonHexChars(string address)
        {
            if (string.IsNullOrEmpty(address)) return "0";
            int start = 0;
            while (start < address.Length && !IsHexChar(address[start]))
                start++;
            return start < address.Length ? address.Substring(start) : "0";
        }

        private static string StripNonDigitChars(string address)
        {
            if (string.IsNullOrEmpty(address)) return "0";
            int start = 0;
            while (start < address.Length && !char.IsDigit(address[start]))
                start++;
            return start < address.Length ? address.Substring(start) : "0";
        }

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        public Result<DevPlcPointDto> Read(DevPlcPointDto readPlcInfo)
        {
            try
            {
                Prefix prefix = readPlcInfo.Prefix.ToPrefix();
                int addr = prefix.IsHexDevice()
                    ? (int)Convert.ToUInt32(StripNonHexChars(readPlcInfo.Address), 16)
                    : int.Parse(StripNonDigitChars(readPlcInfo.Address));
                int wordLen = readPlcInfo.Length * readPlcInfo.DataType.GetTypeOfShortOffset();

                byte[] req = new Mc1ERead().ToByte(prefix, addr, wordLen,readPlcInfo.DataType);
                byte[] resp = SendWithRetry(readPlcInfo.IpAddress, readPlcInfo.Port, req);
                if (resp == null || resp.Length < 1)
                    throw new Exception("读响应为空");

                if (resp[0] != (byte)(req[0] | 0x80))
                    throw new Exception($"PLC 返回错误: 完成码 0x{resp[0]:X2}");

                byte[] rawData = resp.Length > 1 ? resp.Skip(2).ToArray() : new byte[0];
                if (readPlcInfo.DataType == TypeCode.Boolean)
                {
                    rawData = rawData.SplitByteHighLow4Bit(true);
                }

                readPlcInfo.Value = rawData.ConvertToValues(
                    0,
                    readPlcInfo.DataType,
                    readPlcInfo.Length
                );
                return Result<DevPlcPointDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning(
                    "[1E]读取异常：{Ip}-{Port}-{Prefix}{Addr} {Ex}",
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    readPlcInfo.Prefix,
                    readPlcInfo.Address,
                    ex
                );
                readPlcInfo.Value = Enumerable.Repeat<object>(0, readPlcInfo.Length).ToList();
                return Result<DevPlcPointDto>.Fail(ex.Message);
            }
        }

        public Result<List<DevPlcPointDto>> Read(List<DevPlcPointDto> readPlcInfo)
        {
            try
            {
                var items = readPlcInfo
                    .Select(x => new
                    {
                        Dto = x,
                        Prefix = x.Prefix.ToPrefix(),
                    })
                    .Select(x => new
                    {
                        x.Dto,
                        x.Prefix,
                        Addr = x.Prefix.IsHexDevice()
                            ? (int)Convert.ToUInt32(StripNonHexChars(x.Dto.Address), 16)
                            : int.Parse(StripNonDigitChars(x.Dto.Address)),
                        WordLen = x.Dto.DataType == TypeCode.String
                            ? (int)Math.Ceiling((double)x.Dto.Length / 2)
                            : x.Dto.Length * x.Dto.DataType.GetTypeOfShortOffset(),
                    })
                    .ToList();

                foreach (var group in items.GroupBy(x => new { x.Dto.IpAddress, x.Dto.Port }))
                {
                    Prefix prefix = group.First().Prefix;

                    foreach (var typeGroup in group.GroupBy(x => x.Dto.DataType == TypeCode.Boolean))
                    {
                        bool isBool = typeGroup.Key;
                        var sorted = typeGroup.OrderBy(x => x.Addr).ToList();
                        int startAddr = sorted.Min(x => x.Addr);
                        var last = sorted.Last();
                        int endAddr = last.Addr + last.WordLen;
                        int total = endAddr - startAddr;

                        byte[] req = new Mc1ERead().ToByte(
                            prefix,
                            startAddr,
                            total,
                            isBool ? TypeCode.Boolean : TypeCode.Object
                        );
                        byte[] resp = SendWithRetry(group.Key.IpAddress, group.Key.Port, req);

                        if (resp == null || resp.Length < 1)
                            throw new Exception("读响应为空");

                        if (resp[0] != (byte)(req[0] | 0x80))
                            throw new Exception($"PLC 返回错误: 完成码 0x{resp[0]:X2}");

                        byte[] rawData = resp.Length > 1 ? resp.Skip(2).ToArray() : new byte[0];

                        if (isBool)
                        {
                            rawData = rawData.SplitByteHighLow4Bit(true);
                        }

                        foreach (var item in sorted)
                        {
                            int elemByteLen = item.Dto.DataType.GetTypeByteLength();
                            int offset = (item.Addr - startAddr) * item.Dto.DataType.GetTypeOfShortOffset();
                            byte[] elemBytes = new byte[elemByteLen * item.Dto.Length];
                            Array.Copy(
                                rawData,
                                offset,
                                elemBytes,
                                0,
                                Math.Min(elemBytes.Length, rawData.Length - offset)
                            );
                            item.Dto.Value = elemBytes.ConvertToValues(
                                0,
                                item.Dto.DataType,
                                item.Dto.Length
                            );
                        }
                    }
                }

                return Result<List<DevPlcPointDto>>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[1E]批量读取异常：{Ex}", ex.Message);
                return Result<List<DevPlcPointDto>>.Fail(ex.Message);
            }
        }

        #endregion

        #region 写入

        public Result Write(DevPlcPointDto devPlcPointMcDto)
        {
            try
            {
                Prefix prefix = devPlcPointMcDto.Prefix.ToPrefix();

                string addr = prefix.IsHexDevice()
                    ? StripNonHexChars(devPlcPointMcDto.Address)
                    : StripNonDigitChars(devPlcPointMcDto.Address);

                byte[] frame = new Mc1EWirte().ToByte(
                    prefix,
                    addr,
                    devPlcPointMcDto.DataType,
                    devPlcPointMcDto.Value
                );

                byte[] resp = SendWithRetry(
                    devPlcPointMcDto.IpAddress,
                    devPlcPointMcDto.Port,
                    frame
                );

                if (resp.Length < 1 || resp[0] != (byte)(frame[0] | 0x80))
                {
                    string errCode = resp.Length >= 1 ? $"0x{resp[0]:X2}" : "空";
                    throw new Exception($"PLC 写入返回错误完成码: {errCode}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Warning(
                    "[1E]写入异常：{Ip}-{Port}-{Prefix}{Addr} {Ex}",
                    devPlcPointMcDto.IpAddress,
                    devPlcPointMcDto.Port,
                    devPlcPointMcDto.Prefix,
                    devPlcPointMcDto.Address,
                    ex
                );
                return Result.Fail(ex.Message);
            }
        }

        public Result Write(List<DevPlcPointDto> pointMcWriteDto)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 协议核心

        /// <summary>
        /// TCP 收发 + 重试
        /// </summary>
        private byte[] SendWithRetry(string ip, int port, byte[] request)
        {
            for (int i = 0; i < MaxRetry; i++)
            {
                try
                {
                    var client = GetTcpClient(ip, port);
                    var stream = client.GetStream();
                    stream.Write(request, 0, request.Length);

                    using (var ms = new MemoryStream())
                    {
                        byte[] buf = new byte[4096];
                        int read = stream.Read(buf, 0, buf.Length);
                        ms.Write(buf, 0, read);
                        while (stream.DataAvailable)
                        {
                            read = stream.Read(buf, 0, buf.Length);
                            ms.Write(buf, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    if (i == MaxRetry - 1)
                        throw new Exception(
                            $"1E 通信失败({ip}:{port})，已重试{MaxRetry}次: {ex.Message}",
                            ex
                        );
                    MarkInvalid(ip, port);
                    Thread.Sleep(RetryMs);
                }
            }
            throw new Exception("1E 通信重试耗尽");
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 获取或创建 TcpClient 连接（线程安全），支持连接超时
        /// </summary>
        private TcpClient GetTcpClient(string ip, int port)
        {
            string key = $"{ip}:{port}";
            lock (_lockObj)
            {
                if (_tcpDic.TryGetValue(key, out TcpClient client))
                {
                    if (client.Connected)
                        return client;
                    SafeDispose(client);
                    _tcpDic.Remove(key);
                }

                var newClient = new TcpClient();
                var connectTask = newClient.ConnectAsync(ip, port);
                if (!connectTask.Wait(TimeSpan.FromMilliseconds(TimeoutMs)))
                {
                    SafeDispose(newClient);
                    throw new TimeoutException($"连接 PLC {ip}:{port} 超时 ({TimeoutMs}ms)");
                }
                newClient.ReceiveTimeout = TimeoutMs;
                newClient.SendTimeout = TimeoutMs;
                _tcpDic[key] = newClient;
                return newClient;
            }
        }

        /// <summary>
        /// 标记连接无效并从池中移除（线程安全）
        /// </summary>
        private void MarkInvalid(string ip, int port)
        {
            string key = $"{ip}:{port}";
            lock (_lockObj)
            {
                if (_tcpDic.TryGetValue(key, out TcpClient old))
                {
                    _tcpDic.Remove(key);
                    SafeDispose(old);
                }
            }
        }

        private static void SafeDispose(TcpClient client)
        {
            try
            {
                client.Dispose();
            }
            catch { }
        }

        #endregion
    }
}
