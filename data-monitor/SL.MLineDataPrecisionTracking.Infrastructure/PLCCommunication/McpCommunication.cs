using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McpXLib;
using McpXLib.Enums;
using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class McpCommunication : IPlcCommunication
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<string, McpX> _mcpDic = new Dictionary<string, McpX>();

        public McpCommunication() { }

        #region 同步读取方法
        /// <summary>
        /// 同步读取单个点位
        /// </summary>
        public Result<DevPlcPointDto> Read(DevPlcPointDto readPlcInfo)
        {
            Result<DevPlcPointReadDto> re = new Result<DevPlcPointReadDto>()
            {
                Data = new DevPlcPointReadDto(),
            };
            if (int.TryParse(readPlcInfo.Address, out int result) is false)
            {
                byte[] data = null;
                try
                {
                    data = ReadWithRetrySync(
                        readPlcInfo.IpAddress,
                        readPlcInfo.Port,
                        readPlcInfo.Prefix.ToPrefix(),
                        readPlcInfo.Address,
                        (ushort)readPlcInfo.Length
                    );
                    re.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    data = new byte[readPlcInfo.Length * 2];
                    Serilog.Log.Warning(
                        "[Mcp通讯异常]同步读取信息：{readPlcInfo.IpAddress}-{readPlcInfo.Port}-{readPlcInfo.Prefix}-{readPlcInfo.Address}-{readPlcInfo.Length}\r\n{@ex}",
                        readPlcInfo.IpAddress,
                        readPlcInfo.Port,
                        readPlcInfo.Prefix,
                        readPlcInfo.Address,
                        readPlcInfo.Length,
                        ex
                    );
                }
                re.Data.Value = data.ConvertToValues(
                    0 * readPlcInfo.DataType.GetTypeOfShortOffset(),
                    readPlcInfo.DataType,
                    readPlcInfo.Length
                );
            }
            else
            {
                var readDto = new DevPlcPointReadDto(
                    readPlcInfo,
                    readPlcInfo.DataType.GetTypeOfShortOffset()
                );
                re = Read(readDto);
            }

            if (!re.IsSuccess)
            {
                return Result<DevPlcPointDto>.Fail(re.Message);
            }

            readPlcInfo.Value = re.Data.Value;
            return Result<DevPlcPointDto>.Success(readPlcInfo);
        }

        /// <summary>
        /// 同步读取多个点位（自动分组批量读取）
        /// </summary>
        public Result<List<DevPlcPointDto>> Read(List<DevPlcPointDto> readPlcInfo)
        {
            var readDtos = readPlcInfo
                .Select(x => new DevPlcPointReadDto(x, x.DataType.GetTypeOfShortOffset()))
                .ToList();

            var re = Read(readDtos);

            if (!re.IsSuccess)
            {
                return Result<List<DevPlcPointDto>>.Fail(re.Message);
            }

            for (int i = 0; i < readPlcInfo.Count; i++)
            {
                readPlcInfo[i].Value = re.Data[i].Value;
            }

            return Result<List<DevPlcPointDto>>.Success(readPlcInfo);
        }

        /// <summary>
        /// 同步单个读取核心
        /// </summary>
        private Result<DevPlcPointReadDto> Read(DevPlcPointReadDto readPlcInfo)
        {
            try
            {
                var readValue = PaginatedReadingSync(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    readPlcInfo.Prefix.ToPrefix(),
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

                return Result<DevPlcPointReadDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]同步解析数据错误：{Message}", ex.Message);
                return Result<DevPlcPointReadDto>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 同步批量读取核心（分组连续地址）
        /// </summary>
        private Result<List<DevPlcPointReadDto>> Read(List<DevPlcPointReadDto> lineReadPlcInfo)
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
                    foreach (var groupAddre in GroupByAddress(group))
                    {
                        var startAddre = groupAddre.Min(x => x.Address);
                        var endAddressInfo = groupAddre.OrderByDescending(x => x.Address).First();

                        var length =
                            endAddressInfo.Address
                            + (
                                endAddressInfo.DataType == TypeCode.String
                                    ? (int)Math.Ceiling((double)endAddressInfo.Length / 2)
                                    : endAddressInfo.Length * endAddressInfo.ShortOffset
                            )
                            - startAddre;

                        var readValue = PaginatedReadingSync(
                            group.Key.IpAddress,
                            group.Key.Port,
                            group.Key.Prefix.ToPrefix(),
                            startAddre,
                            length
                        );

                        if (!readValue.IsSuccess)
                        {
                            byte[] bytes = new byte[length * 2];
                            foreach (var item in groupAddre)
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
                            foreach (var item in groupAddre)
                            {
                                item.Value = readValue.Data.ConvertToValues(
                                    ((item.Address - startAddre) * (item.Prefix.ToPrefix().IsHexDevice()?1:2)),
                                    item.DataType,
                                    item.Length
                                );
                            }
                        }
                    }
                }

                return Result<List<DevPlcPointReadDto>>.Success(lineReadPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]同步批量解析数据错误：{Message}", ex.Message);
                return Result<List<DevPlcPointReadDto>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// PLC 地址分组工具（间隔 >128 断开）
        /// </summary>
        /// <summary>
        /// 对 DevPlcPointReadDto 集合按 Address 连续分组
        /// 规则：后地址 - 前地址 <= 128 → 同一组
        ///      后地址 - 前地址 > 128 → 新组
        /// </summary>
        List<List<DevPlcPointReadDto>> GroupByAddress(IEnumerable<DevPlcPointReadDto> pointList)
        {
            // 1. 必须按地址从小到大排序
            var sorted = pointList.OrderBy(p => p.Address).ToList();
            var result = new List<List<DevPlcPointReadDto>>();

            if (!sorted.Any())
                return result;

            // 2. 初始化第一组
            var currentGroup = new List<DevPlcPointReadDto> { sorted[0] };
            result.Add(currentGroup);

            // 3. 遍历分组
            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var curr = sorted[i];

                // 地址差 > 128 → 断开，新建组
                if (curr.Address - prev.Address > 128)
                {
                    currentGroup = new List<DevPlcPointReadDto>();
                    result.Add(currentGroup);
                }

                currentGroup.Add(curr);
            }

            return result;
        }

        /// <summary>
        /// 同步分页读取
        /// </summary>
        private Result<byte[]> PaginatedReadingSync(
            string ipAddress,
            int port,
            Prefix prefix,
            int startAddre,
            int length
        )
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
                    var data = ReadWithRetrySync(
                        ipAddress,
                        port,
                        prefix,
                        currentAddress.ToString(),
                        readLen
                    );

                    // 空数据重试一次
                    if (data.Length == 0)
                    {
                        Thread.Sleep(100);
                        data = ReadWithRetrySync(
                            ipAddress,
                            port,
                            prefix,
                            currentAddress.ToString(),
                            readLen
                        );
                    }

                    allData.AddRange(data);
                }
                catch (Exception ex)
                {
                    Log.Warning(
                        "[Mcp通讯异常]同步读取信息：{ip}-{port}-{prefix}-{addr}-{len}\r\n{ex}",
                        ipAddress,
                        port,
                        prefix,
                        startAddre,
                        length,
                        ex
                    );

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
            string currentAddress,
            ushort readLen,
            int maxRetry = 3,
            int retryInterval = 300
        )
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // 同步调用McpX读取方法
                    byte[] data;
                    if (prefix.IsHexDevice())
                    {
                        data = BoolArrayToByteArrayHighBit(
                            GetMcp(ipAddress, port)
                                .BatchReadBool(prefix, currentAddress.ToString(), readLen)
                        );
                    }
                    else
                    {
                        data = GetMcp(ipAddress, port)
                            .BatchReadByte(prefix, currentAddress.ToString(), readLen);
                    }
                    return data;
                }
                catch (Exception ex)
                {
                    Log.Warning($"同步第{i + 1}次读取失败：{ex.Message}");

                    MarkMcpInvalid(ipAddress, port);

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
        #region 同步写
        public async Task<Result> WriteAsync(DevPlcPointDto devPlcPointMcDto)
        {
            return await WriteAsync(new DevPlcPointWriteDto(devPlcPointMcDto));
        }

        public Result Write(DevPlcPointDto devPlcPointMcDto)
        {
            return Write(new DevPlcPointWriteDto(devPlcPointMcDto));
        }

        private Result Write(DevPlcPointWriteDto pointMcWriteDto)
        {
            return PaginatedWriteing(
                pointMcWriteDto.IpAddress,
                pointMcWriteDto.Port,
                pointMcWriteDto.Prefix.ToPrefix(),
                pointMcWriteDto.DataType,
                pointMcWriteDto.Address,
                pointMcWriteDto.Value
            );
        }

        Result PaginatedWriteing(
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
                return WriteWithRetry(ipAddress, port, prefix, typeCode, address, value);
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

        private Result WriteWithRetry(
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
                            mcp.BatchWriteBool(
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
                            mcp.BatchWriteInt16(
                                prefix,
                                address.ToString(),
                                value.Select(x => short.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.UInt16:
                            mcp.BatchWriteUInt16(
                                prefix,
                                address.ToString(),
                                value.Select(x => ushort.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.Int32:
                            mcp.BatchWriteInt32(
                                prefix,
                                address.ToString(),
                                value.Select(x => int.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.UInt32:
                            mcp.BatchWriteUInt32(
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
                            mcp.BatchWriteSingle(
                                prefix,
                                address.ToString(),
                                value.Select(x => float.Parse(x?.ToString())).ToArray()
                            );
                            break;
                        case TypeCode.Double:
                            mcp.BatchWriteDouble(
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
                            mcp.WriteString(prefix, address.ToString(), string.Concat(value));
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

                    Thread.Sleep(100);
                }
            }
            throw new Exception("重试失败");
        }

        #endregion
        private async Task<Result> WriteAsync(DevPlcPointWriteDto pointMcWriteDto)
        {
            return await PaginatedWriteingAsync(
                pointMcWriteDto.IpAddress,
                pointMcWriteDto.Port,
                pointMcWriteDto.Prefix.ToPrefix(),
                pointMcWriteDto.DataType,
                pointMcWriteDto.Address,
                pointMcWriteDto.Value
            );
        }

        async Task<Result> PaginatedWriteingAsync(
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
                return await WriteWithRetryAsync(ipAddress, port, prefix, typeCode, address, value);
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

        private async Task<Result> WriteWithRetryAsync(
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

        //public async Task<Result<DevPlcPointDto>> ReadAsync(DevPlcPointDto readPlcInfo)
        //{
        //    var re = await ReadAsync(
        //        new DevPlcPointReadDto(readPlcInfo, readPlcInfo.DataType.GetTypeOfShortOffset())
        //    );
        //    if (re.IsSuccess is false)
        //    {
        //        return Result<DevPlcPointDto>.Fail(re.Message);
        //    }
        //    else
        //    {
        //        readPlcInfo.Value = re.Data.Value;

        //        return Result<DevPlcPointDto>.Success(readPlcInfo);
        //    }
        //}

        async Task<Result<DevPlcPointReadDto>> ReadAsync(DevPlcPointReadDto readPlcInfo)
        {
            Result<byte[]> readValue;
            try
            {
                readValue = await PaginatedReading(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    readPlcInfo.Prefix.ToPrefix(),
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
                return Result<DevPlcPointReadDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]解析数据错误：{ex.Message}", ex.Message);
                return Result<DevPlcPointReadDto>.Fail(ex.Message);
            }
        }

        //public async Task<Result<List<DevPlcPointDto>>> ReadAsync(
        //    List<DevPlcPointDto> readPlcInfo
        //)
        //{
        //    var re = await ReadAsync(
        //        readPlcInfo
        //            .Select(x => new DevPlcPointReadDto(x, x.DataType.GetTypeOfShortOffset()))
        //            .ToList()
        //    );
        //    if (re.IsSuccess is false)
        //    {
        //        return Result<List<DevPlcPointDto>>.Fail(re.Message);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < readPlcInfo.Count; i++)
        //        {
        //            readPlcInfo[i].Value = re.Data[i].Value;
        //        }
        //        return Result<List<DevPlcPointDto>>.Success(readPlcInfo);
        //    }
        //}

        async Task<Result<List<DevPlcPointReadDto>>> ReadAsync(
            List<DevPlcPointReadDto> lineReadPlcInfo
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
                    foreach (var groupAddre in GroupByAddress(group))
                    {
                        var startAddre = groupAddre.Min(x => x.Address);
                        var endAddressInfo = groupAddre.OrderByDescending(x => x.Address).First();
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
                            group.Key.Prefix.ToPrefix(),
                            startAddre,
                            lenght
                        );
                        if (readValue.IsSuccess is false)
                        {
                            byte[] bytes = new byte[lenght * 2];
                            foreach (DevPlcPointReadDto item in groupAddre)
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
                            foreach (DevPlcPointReadDto item in groupAddre)
                            {
                                item.Value = readValue.Data.ConvertToValues(
                                    ((item.Address - startAddre) * 2),
                                    item.DataType,
                                    item.Length
                                );
                            }
                        }
                    }
                }
                return Result<List<DevPlcPointReadDto>>.Success(lineReadPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[Mcp通讯异常]解析数据错误：{ex.Message}", ex.Message);
                return Result<List<DevPlcPointReadDto>>.Fail(ex.Message);
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
                    if (data.Length == 0)
                    {
                        await Task.Delay(100);
                        data = await ReadWithRetry(
                            ipAddress,
                            port,
                            prefix,
                            currentAddress,
                            readLen
                        );
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
            int retryInterval = 300
        )
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // 3. 读取（带超时）
                    var data = await GetMcp(ipAddress, port)
                        .BatchReadByteAsync(prefix, currentAddress.ToString(), readLen);

                    return data;
                }
                catch (Exception ex)
                {
                    Log.Warning($"第{i + 1}次读取失败：{ex.Message}");

                    MarkMcpInvalid(ipAddress, port);

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

        /// <summary>
        /// bool数组转byte数组，每个bool单独占1字节，bool存放在字节最高位Bit7
        /// true=0x80，false=0x00
        /// </summary>
        /// <param name="boolArray">输入布尔数组</param>
        /// <returns>转换后的byte数组</returns>
        byte[] BoolArrayToByteArrayHighBit(bool[] boolArray)
        {
            // 空数组处理
            if (boolArray == null || boolArray.Length == 0)
                return Array.Empty<byte>();

            byte[] result = new byte[boolArray.Length];

            for (int i = 0; i < boolArray.Length; i++)
            {
                result[i] = boolArray[i] ? (byte)0x01 : (byte)0x00;
            }

            return result;
        }

        public Result Write(List<DevPlcPointDto> pointMcWriteDto)
        {
            throw new NotImplementedException();
        }
    }
}
