using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using McpXLib;
using McpXLib.Enums;
using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class McpCommunication
    {
        Dictionary<string, McpX> _mcpDic;
        private readonly object _lockObj = new object();

        public McpCommunication()
        {
            _mcpDic = new Dictionary<string, McpX>();
        }

         McpX GetMcp(string ipAddress, int port)
        {
            string key = $"{ipAddress}:{port}";

            lock (_lockObj) // 多线程安全必须加
            {
                // 1. 存在就先返回
                if (_mcpDic.TryGetValue(key, out McpX mcp))
                {
                    return mcp;
                }

                // 2. 不存在就创建
                var newMcp = new McpX(ipAddress, port);
                _mcpDic[key] = newMcp;
                return newMcp;
            }
        }

        public async Task<Result<List<DevPlcPointMcDto>>> ReadAsync(
            List<DevPlcPointMcDto> lineReadPlcInfo
        )
        {
            var re = await ReadAsync(
                lineReadPlcInfo
                    .Select(x => new DevPlcPointMcReadDto(x, x.DataType.GetTypeOfShortOffset()))
                    .ToList()
            );
            if (re.IsSuccess is false)
            {
                return Result<List<DevPlcPointMcDto>>.Fail(re.Message);
            }
            else
            {
                for (int i = 0; i < lineReadPlcInfo.Count; i++)
                {
                    lineReadPlcInfo[i].Value = re.Data[i].Value;
                }
                return Result<List<DevPlcPointMcDto>>.Success(lineReadPlcInfo);
            }
        }

        async Task<Result<List<DevPlcPointMcReadDto>>> ReadAsync(
            List<DevPlcPointMcReadDto> lineReadPlcInfo
        )
        {
            try
            {
                var groups = lineReadPlcInfo.GroupBy(x => new
                {
                    x.IpAddress,
                    x.Port,
                    x.Prefix,
                });
                foreach (var group in groups)
                {
                    var startAddre = group.Min(x => x.Address);
                    var endAddressInfo = group.OrderByDescending(x => x.Address).First();
                    //mcpx 是按照short 也就是可读取最小寄存器 来解析的所以要加上shourt偏移
                    var lenght =
                        endAddressInfo.Address
                        + (endAddressInfo.Length * endAddressInfo.ShortOffset)
                        - startAddre;

                    var readValue = await PaginatedReading(
                        group.Key.IpAddress,
                        group.Key.Port,
                        group.Key.Prefix,
                        startAddre,
                        lenght
                    );
                    if (readValue.IsSuccess is false)
                    {
                        byte[] bytes = new byte[lenght*2];
                        foreach (DevPlcPointMcReadDto item in group)
                        {
                            item.Value = bytes.ConvertToValues(
                                (item.Address - startAddre) * item.ShortOffset,
                                item.DataType,
                                item.Length
                            );
                        }
                    }
                    else
                    {
                        foreach (DevPlcPointMcReadDto item in group)
                        {
                            item.Value = readValue.Data.ConvertToValues(
                                (item.Address - startAddre) * item.ShortOffset,
                                item.DataType,
                                item.Length
                            );
                        }
                    }
                }
                return Result<List<DevPlcPointMcReadDto>>.Success(lineReadPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]解析数据错误：{ex.Message}", ex.Message);
                return Result<List<DevPlcPointMcReadDto>>.Fail(ex.Message);
            }
        }

        private async Task<Result<byte[]>> PaginatedReading(
            string ipAddress,
            int port,
            Prefix prefix,
            int startAddre,
            int lenght
        )
        {
            Result<byte[]> result = new Result<byte[]>() { IsSuccess = true };
            // 开始地址
            int currentAddress = startAddre;
            // 剩余长度
            int remaining = lenght;
            // 存储所有读取结果
            List<byte> allData = new List<byte>();

            // 自动循环分页读取
            while (remaining > 0)
            {
                // 本次读取长度：最多 65535
                ushort readLen = (ushort)Math.Min(remaining, ushort.MaxValue);

                try
                {
                    var data = await ReadWithRetry(
                        ipAddress,
                        port,
                        prefix,
                        currentAddress,
                        readLen
                    );

                    // 把读到的数据加入总结果
                    allData.AddRange(data);
                }
                catch (Exception ex)
                {
                    Log.Warning(
                        "[Mcp通讯异常]读取信息：{ipAddress}-{port}-{prefix}-{startAddre}-{lenght}。\r\n{ex}",
                        ipAddress,
                        port,
                        prefix,
                        startAddre,
                        lenght,
                        ex
                    );
                    allData.AddRange(new byte[readLen]);
                    result.IsSuccess = false;
                }

                // 偏移地址
                currentAddress += readLen;
                // 减少剩余长度
                remaining -= readLen;
            }
            result.Data = allData.ToArray();
            // 最终所有数据在这里
            return result;
        }

        private void MarkMcpInvalid(string ipAddress, int port)
        {
            string key = $"{ipAddress}:{port}";

            lock (_lockObj)
            {
                if (_mcpDic.TryGetValue(key, out var oldMcp))
                {
                    try
                    {
                        oldMcp.Dispose();
                        _mcpDic.Remove(key);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 带重试 + 自动重建连接的读取
        /// </summary>
        async Task<byte[]> ReadWithRetry(
            string ipAddress,
            int port,
            Prefix prefix,
            int currentAddress,
            ushort readLen,
            int maxRetry = 3, // 最多重试次数
            int retryInterval = 300
        ) // 重试间隔毫秒
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // 调用纯业务方法
                    return await GetMcp(ipAddress, port)
                        .BatchReadByteAsync(prefix, currentAddress.ToString(), readLen);
                }
                catch (Exception ex)
                {
                    // 最后一次还失败 → 不重试了
                    if (i == maxRetry - 1)
                        throw new Exception($"读取失败，已重试{maxRetry}次：{ex.Message}", ex);

                    // 出现异常 → 标记连接失效（下次自动新建）
                    MarkMcpInvalid(ipAddress, port);

                    // 等待后重试
                    await Task.Delay(retryInterval);
                }
            }

            throw new Exception("重试失败");
        }
    }
}
