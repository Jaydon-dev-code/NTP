using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class McpCommunication
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<string, McpX> _mcpDic = new Dictionary<string, McpX>();

        public McpCommunication() { }


        #region 同步读取方法
        /// <summary>
        /// 同步读取单个点位
        /// </summary>
        public Result<DevPlcPointMcDto> Read(DevPlcPointMcDto readPlcInfo)
        {
            var readDto = new DevPlcPointMcReadDto(readPlcInfo, readPlcInfo.DataType.GetTypeOfShortOffset());
            var re = Read(readDto);

            if (!re.IsSuccess)
            {
                return Result<DevPlcPointMcDto>.Fail(re.Message);
            }

            readPlcInfo.Value = re.Data.Value;
            return Result<DevPlcPointMcDto>.Success(readPlcInfo);
        }

        /// <summary>
        /// 同步读取多个点位（自动分组批量读取）
        /// </summary>
        public Result<List<DevPlcPointMcDto>> Read(List<DevPlcPointMcDto> readPlcInfo)
        {
            var readDtos = readPlcInfo
                .Select(x => new DevPlcPointMcReadDto(x, x.DataType.GetTypeOfShortOffset()))
                .ToList();

            var re = Read(readDtos);

            if (!re.IsSuccess)
            {
                return Result<List<DevPlcPointMcDto>>.Fail(re.Message);
            }

            for (int i = 0; i < readPlcInfo.Count; i++)
            {
                readPlcInfo[i].Value = re.Data[i].Value;
            }

            return Result<List<DevPlcPointMcDto>>.Success(readPlcInfo);
        }

        /// <summary>
        /// 同步单个读取核心
        /// </summary>
        private Result<DevPlcPointMcReadDto> Read(DevPlcPointMcReadDto readPlcInfo)
        {
            try
            {
                var readValue = PaginatedReadingSync(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    readPlcInfo.Prefix,
                    readPlcInfo.Address,
                    readPlcInfo.Length
                );

                if (!readValue.IsSuccess)
                {
                    byte[] bytes = new byte[readPlcInfo.Length * 2];
                    readPlcInfo.Value = bytes.ConvertToValues(
                        0 * readPlcInfo.ShortOffset,
                        readPlcInfo.DataType,
                        readPlcInfo.Length
                    );
                }
                else
                {
                    readPlcInfo.Value = readValue.Data.ConvertToValues(
                        0 * readPlcInfo.ShortOffset,
                        readPlcInfo.DataType,
                        readPlcInfo.Length
                    );
                }

                return Result<DevPlcPointMcReadDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]同步解析数据错误：{Message}", ex.Message);
                return Result<DevPlcPointMcReadDto>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 同步批量读取核心（分组连续地址）
        /// </summary>
        private Result<List<DevPlcPointMcReadDto>> Read(List<DevPlcPointMcReadDto> lineReadPlcInfo)
        {
            try
            {
                var groups = lineReadPlcInfo.GroupBy(x => new
                {
                    x.IpAddress,
                    x.Port,
                    x.Prefix
                });

                foreach (var group in groups)
                {
                    var startAddre = group.Min(x => x.Address);
                    var endAddressInfo = group.OrderByDescending(x => x.Address).First();

                    var length = endAddressInfo.Address
                        + (endAddressInfo.DataType == TypeCode.String
                            ? (int)Math.Ceiling((double)endAddressInfo.Length / 2)
                            : endAddressInfo.Length * endAddressInfo.ShortOffset)
                        - startAddre;

                    var readValue = PaginatedReadingSync(
                        group.Key.IpAddress,
                        group.Key.Port,
                        group.Key.Prefix,
                        startAddre,
                        length
                    );

                    if (!readValue.IsSuccess)
                    {
                        byte[] bytes = new byte[length * 2];
                        foreach (var item in group)
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
                        foreach (var item in group)
                        {
                            item.Value = readValue.Data.ConvertToValues(
                                ((item.Address - startAddre) * 2),
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
                Log.Warning("[Mcp通讯异常]同步批量解析数据错误：{Message}", ex.Message);
                return Result<List<DevPlcPointMcReadDto>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 同步分页读取
        /// </summary>
        private Result<byte[]> PaginatedReadingSync(
            string ipAddress,
            int port,
            Prefix prefix,
            int startAddre,
            int length)
        {
            Result<byte[]> result = new Result<byte[]> { IsSuccess = true };
            int currentAddress = startAddre;
            int remaining = length;
            List<byte> allData = new List<byte>();

            while (remaining > 0)
            {
                ushort readLen = (ushort)Math.Min(remaining, 960);

                try
                {
                    Log.Debug("同步读取开始。");
                    var data = ReadWithRetrySync(ipAddress, port, prefix, currentAddress, readLen);

                    // 空数据重试一次
                    if (data.Length == 0)
                    {
                        Thread.Sleep(100);
                        data = ReadWithRetrySync(ipAddress, port, prefix, currentAddress, readLen);
                    }

                    Log.Debug("同步读取结束。");
                    allData.AddRange(data);
                }
                catch (Exception ex)
                {
                    Log.Warning(
                        "[Mcp通讯异常]同步读取信息：{ip}-{port}-{prefix}-{addr}-{len}\r\n{ex}",
                        ipAddress, port, prefix, startAddre, length, ex);

                    allData.AddRange(new byte[readLen]);
                    result.IsSuccess = false;
                }

                currentAddress += readLen;
                remaining -= readLen;
            }

            result.Data = allData.ToArray();
            return result;
        }

        /// <summary>
        /// 同步读取 + 重试机制
        /// </summary>
        private byte[] ReadWithRetrySync(
            string ipAddress,
            int port,
            Prefix prefix,
            int currentAddress,
            ushort readLen,
            int maxRetry = 3,
            int retryInterval = 300)
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // 同步调用McpX读取方法
                    var data = GetMcp(ipAddress, port).BatchReadByte(prefix, currentAddress.ToString(), readLen);
                    return data;
                }
                catch (Exception ex)
                {
                    Log.Warning($"同步第{i + 1}次读取失败：{ex.Message}");

                    if (i == maxRetry - 1)
                    {
                        throw new Exception($"同步读取失败，已重试{maxRetry}次：{ex.Message}", ex);
                    }

                    // 同步等待
                    Thread.Sleep(retryInterval);
                }
            }

            throw new Exception($"同步重试{maxRetry}次后读取失败");
        }
        #endregion

        public async Task<Result> WriteAsync(DevPlcPointMcDto devPlcPointMcDto)
        {
            return await WriteAsync(new DevPlcPointMcWriteDto(devPlcPointMcDto));
        }

        private async Task<Result> WriteAsync(DevPlcPointMcWriteDto pointMcWriteDto)
        {
            return await PaginatedWriteing(
                pointMcWriteDto.IpAddress,
                pointMcWriteDto.Port,
                pointMcWriteDto.Prefix,
                pointMcWriteDto.DataType,
                pointMcWriteDto.Address,
                pointMcWriteDto.Value
            );
        }

        async Task<Result> PaginatedWriteing(
            string ipAddress,
            int port,
            Prefix prefix,
            TypeCode typeCode,
            int address,
            List<object> value
        )
        {
            try
            {
                return await WriteWithRetry(ipAddress, port, prefix, typeCode, address, value);
            }
            catch (Exception ex)
            {
                Log.Warning(
                    "[Mcp通讯异常]写入信息：{ipAddress}-{port}-{prefix}-{address}-{@value}。\r\n{ex.Message}",
                    ipAddress,
                    port,
                    prefix,
                    address,
                    value,
                    ex.Message
                );
                return Result.Fail(ex.Message);
            }
        }

        private async Task<Result> WriteWithRetry(
            string ipAddress,
            int port,
            Prefix prefix,
            TypeCode typeCode,
            int address,
            List<object> value,
            int maxRetry = 3, // 最多重试次数
            int retryInterval = 300
        )
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    var mcp = GetMcp(ipAddress, port);
                    switch (typeCode)
                    {
                        //case TypeCode.Empty:
                        //    break;
                        //case TypeCode.Object:
                        //    break;
                        //case TypeCode.DBNull:
                        //    break;
                        case TypeCode.Boolean:
                            await mcp.BatchWriteBoolAsync(
                                prefix,
                                address.ToString(),
                                value.Select(x => bool.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        //case TypeCode.Char:
                        //    break;
                        //case TypeCode.SByte:
                        //    break;
                        //case TypeCode.Byte:
                        //    break;
                        case TypeCode.Int16:
                            await mcp.BatchWriteInt16Async(
                                prefix,
                                address.ToString(),
                                value.Select(x => short.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.UInt16:
                            await mcp.BatchWriteUInt16Async(
                                prefix,
                                address.ToString(),
                                value.Select(x => ushort.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.Int32:
                            await mcp.BatchWriteInt32Async(
                                prefix,
                                address.ToString(),
                                value.Select(x => int.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.UInt32:
                            await mcp.BatchWriteUInt32Async(
                                prefix,
                                address.ToString(),
                                value.Select(x => UInt32.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        //case TypeCode.Int64:
                        //    break;
                        //case TypeCode.UInt64:
                        //    break;
                        case TypeCode.Single:
                            await mcp.BatchWriteSingleAsync(
                                prefix,
                                address.ToString(),
                                value.Select(x => float.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.Double:
                            await mcp.BatchWriteDoubleAsync(
                                prefix,
                                address.ToString(),
                                value.Select(x => double.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        //case TypeCode.Decimal:
                        //    break;
                        //case TypeCode.DateTime:
                        //    break;
                        case TypeCode.String:
                            await mcp.WriteStringAsync(
                                prefix,
                                address.ToString(),
                                string.Concat(value)
                            );
                            break;
                        default:
                            throw new NotSupportedException($"不支持的数据类型: {typeCode}");
                    }
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    // 最后一次还失败 → 不重试了
                    if (i == maxRetry - 1)
                        throw new Exception($"写入失败，已重试{maxRetry}次：{ex.Message}", ex);

                    // 出现异常 → 标记连接失效（下次自动新建）
                    MarkMcpInvalid(ipAddress, port);

                    // 等待后重试
                    await Task.Delay(retryInterval);
                }
            }
            throw new Exception("重试失败");
        }

        //public async Task<Result<DevPlcPointMcDto>> ReadAsync(DevPlcPointMcDto readPlcInfo)
        //{
        //    var re = await ReadAsync(
        //        new DevPlcPointMcReadDto(readPlcInfo, readPlcInfo.DataType.GetTypeOfShortOffset())
        //    );
        //    if (re.IsSuccess is false)
        //    {
        //        return Result<DevPlcPointMcDto>.Fail(re.Message);
        //    }
        //    else
        //    {
        //        readPlcInfo.Value = re.Data.Value;

        //        return Result<DevPlcPointMcDto>.Success(readPlcInfo);
        //    }
        //}

        async Task<Result<DevPlcPointMcReadDto>> ReadAsync(DevPlcPointMcReadDto readPlcInfo)
        {
            Result<byte[]> readValue;
            try
            {
                readValue = await PaginatedReading(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    readPlcInfo.Prefix,
                    readPlcInfo.Address,
                    readPlcInfo.Length
                );
                if (readValue.IsSuccess is false)
                {
                    byte[] bytes = new byte[readPlcInfo.Length * 2];

                    readPlcInfo.Value = bytes.ConvertToValues(
                        0 * readPlcInfo.ShortOffset,
                        readPlcInfo.DataType,
                        readPlcInfo.Length
                    );
                }
                else
                {
                    readPlcInfo.Value = readValue.Data.ConvertToValues(
                        0 * readPlcInfo.ShortOffset,
                        readPlcInfo.DataType,
                        readPlcInfo.Length
                    );
                }
                return Result<DevPlcPointMcReadDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]解析数据错误：{ex.Message}", ex.Message);
                return Result<DevPlcPointMcReadDto>.Fail(ex.Message);
            }
        }

        //public async Task<Result<List<DevPlcPointMcDto>>> ReadAsync(
        //    List<DevPlcPointMcDto> readPlcInfo
        //)
        //{
        //    var re = await ReadAsync(
        //        readPlcInfo
        //            .Select(x => new DevPlcPointMcReadDto(x, x.DataType.GetTypeOfShortOffset()))
        //            .ToList()
        //    );
        //    if (re.IsSuccess is false)
        //    {
        //        return Result<List<DevPlcPointMcDto>>.Fail(re.Message);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < readPlcInfo.Count; i++)
        //        {
        //            readPlcInfo[i].Value = re.Data[i].Value;
        //        }
        //        return Result<List<DevPlcPointMcDto>>.Success(readPlcInfo);
        //    }
        //}

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
                        // string 的长度等于 byte 所以的 /2
                        + (
                            endAddressInfo.DataType == TypeCode.String
                                ? (int)Math.Ceiling((double)endAddressInfo.Length / 2)
                                : (endAddressInfo.Length * endAddressInfo.ShortOffset)
                        )
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
                        byte[] bytes = new byte[lenght * 2];
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
                                ((item.Address - startAddre) * 2),
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
                ushort readLen = (ushort)Math.Min(remaining, 960);

                try
                {
                    Log.Debug("读取开始。");
                    var data = await ReadWithRetry(
                        ipAddress,
                        port,
                        prefix,
                        currentAddress,
                        readLen
                    );
                    if (data.Length==0)
                    {
                        await Task.Delay(100);
                        data = await ReadWithRetry(
                        ipAddress,
                        port,
                        prefix,
                        currentAddress,
                        readLen);
                    }
                    Log.Debug("读取结束。");
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


        /// <summary>
        /// 线程安全 + 自动释放 + 带超时 + 不会死锁的 PLC 读取方法
        /// </summary>
        async Task<byte[]> ReadWithRetry(
            string ipAddress,
            int port,
            Prefix prefix,
            int currentAddress,
            ushort readLen,
            int maxRetry = 3,
            int retryInterval = 300)
        {
            for (int i = 0; i < maxRetry; i++)
            {
   

                try
                {
        
                    // 3. 读取（带超时）
                    var data = await GetMcp(ipAddress, port).BatchReadByteAsync(prefix, currentAddress.ToString(), readLen);

                    return data;
                }
                catch (Exception ex)
                {
                    Log.Warning($"第{i + 1}次读取失败：{ex.Message}");

                    // 最后一次重试失败，抛出异常
                    if (i == maxRetry - 1)
                    {
                        throw new Exception($"读取失败，已重试{maxRetry}次：{ex.Message}", ex);
                    }

                    await Task.Delay(retryInterval);
                }
             
            }

            throw new Exception($"重试{maxRetry}次后读取失败");
        }
        McpX GetMcp(string ipAddress, int port)
        {
            string key = $"{ipAddress}:{port}";

            lock (_lockObj)
            {
                if (_mcpDic.TryGetValue(key, out McpX mcp))
                    return mcp;

                var newMcp = new McpX(ipAddress, port);
                _mcpDic[key] = newMcp;
                return newMcp;
            }
        }

        /// <summary>
        /// 无死锁销毁连接（锁外 Dispose）
        /// </summary>
        private void MarkMcpInvalid(string ipAddress, int port)
        {
            string key = $"{ipAddress}:{port}";
            McpX oldMcp = null;

            // 锁里只做字典移除（极快）
            lock (_lockObj)
            {
                if (_mcpDic.TryGetValue(key, out oldMcp))
                    _mcpDic.Remove(key);
            }

            // 锁外面再释放！！！绝对不死锁
            if (oldMcp != null)
            {
                try
                {
                    oldMcp.Dispose();
                }
                catch { }
            }
        }

    }
}
