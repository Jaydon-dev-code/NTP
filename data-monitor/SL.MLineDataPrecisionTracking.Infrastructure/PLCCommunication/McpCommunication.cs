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
            if (int.TryParse(readPlcInfo.Address, result: out int result) is false)
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
                    
                    //data = new byte[readPlcInfo.Length * 2];
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
                //re.Data.Value = data.ConvertToValues(
                //    0 * readPlcInfo.DataType.GetTypeOfShortOffset(),
                //    readPlcInfo.DataType,
                //    readPlcInfo.Length
                //);
                return Result<DevPlcPointDto>.Fail(re.Message);
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
            if (string.IsNullOrEmpty(readPlcInfo?.ReadFormula))
            {
                readPlcInfo.Value = re.Data.Value;
            }
            else
            {
                List<object> tmpValue = new List<object>();
                foreach (var item in re.Data.Value)
                {
                    tmpValue.Add(readPlcInfo.ReadFormula.StringCompute(item.ToString()));
                }
                readPlcInfo.Value = tmpValue;
            }

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
                if (string.IsNullOrEmpty(readPlcInfo[i]?.ReadFormula))
                {
                    readPlcInfo[i].Value = re.Data[i].Value;
                }
                else
                {
                    List<object> tmpValue = new List<object>();
                    foreach (var item in re.Data[i].Value)
                    {
                        tmpValue.Add(readPlcInfo[i]?.ReadFormula.StringCompute(item.ToString()));
                    }
                    readPlcInfo[i].Value = tmpValue;
                }
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
                var prefix = readPlcInfo.Prefix.ToPrefix();
                //int address = prefix.IsHexDevice()
                //    ? (int)Convert.ToUInt32(readPlcInfo.Address, 16)
                //    : int.Parse(readPlcInfo.Address);
                int address = int.Parse(readPlcInfo.Address);
                var readValue = PaginatedReadingSync(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    prefix,
                    address,
                    readPlcInfo.Length,
                    readPlcInfo.DataType
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
                    var prefix = group.Key.Prefix.ToPrefix();
                    bool isHex = prefix.IsHexDevice();

                    foreach (var groupAddre in GroupByAddress(group))
                    {
                        var list = groupAddre
                            .Select(item => new
                            {
                                Item = item,
                                Addr = isHex
                                    ? (int)Convert.ToUInt32(item.Address, 16)
                                    : int.Parse(item.Address),
                            })
                            .ToList();

                        var startAddre = list.Min(x => x.Addr);
                        var endAddressInfo = list.OrderByDescending(x => x.Addr).First();

                        var length =
                            endAddressInfo.Addr
                            + (
                                endAddressInfo.Item.DataType == TypeCode.String
                                    ? (int)Math.Ceiling((double)endAddressInfo.Item.Length / 2)
                                    : endAddressInfo.Item.Length * endAddressInfo.Item.ShortOffset
                            )
                            - startAddre;

                        var typeCode = list.All(x => x.Item.DataType == TypeCode.Boolean)
                            ? TypeCode.Boolean
                            : TypeCode.Object;

                        var readValue = PaginatedReadingSync(
                            group.Key.IpAddress,
                            group.Key.Port,
                            prefix,
                            startAddre,
                            length,
                            typeCode
                        );

                        if (!readValue.IsSuccess)
                        {
                            //byte[] bytes = new byte[length * 2];
                            //foreach (var x in list)
                            //{
                            //    x.Item.Value = bytes.ConvertToValues(
                            //        (x.Addr - startAddre) * x.Item.ShortOffset,
                            //        x.Item.DataType,
                            //        x.Item.Length
                            //    );
                            //}
                            return Result<List<DevPlcPointReadDto>>.Fail(readValue.Message);
                        }
                        else
                        {
                            foreach (var x in list)
                            {
                                x.Item.Value = readValue.Data.ConvertToValues(
                                    (
                                        (x.Addr - startAddre)
                                        * (x.Item.DataType == TypeCode.Boolean ? 1 : 2)
                                    ),
                                    x.Item.DataType,
                                    x.Item.Length
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
            var sorted = pointList
                .Select(p => new { Item = p, Addr = ParseAddress(p.Address, p.Prefix) })
                .OrderBy(p => p.Addr)
                .ToList();

            var result = new List<List<DevPlcPointReadDto>>();

            if (!sorted.Any())
                return result;

            var currentGroup = new List<DevPlcPointReadDto> { sorted[0].Item };
            result.Add(currentGroup);

            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i].Addr - sorted[i - 1].Addr > 128)
                {
                    currentGroup = new List<DevPlcPointReadDto>();
                    result.Add(currentGroup);
                }

                currentGroup.Add(sorted[i].Item);
            }

            return result;
        }

        private static int ParseAddress(string address, string prefix)
        {
            var p = prefix.ToPrefix();
            return p.IsHexDevice() ? (int)Convert.ToUInt32(address, 16) : int.Parse(address);
        }

        /// <summary>
        /// 同步分页读取
        /// </summary>
        private Result<byte[]> PaginatedReadingSync(
            string ipAddress,
            int port,
            Prefix prefix,
            int startAddre,
            int length,
            TypeCode typeCode = TypeCode.Object
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
                        readLen,
                        typeCode
                    );

                    if (data.Length == 0)
                    {
                        Thread.Sleep(100);
                        data = ReadWithRetrySync(
                            ipAddress,
                            port,
                            prefix,
                            currentAddress.ToString(),
                            readLen,
                            typeCode
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
            TypeCode typeCode = TypeCode.Object,
            int maxRetry = 3,
            int retryInterval = 300
        )
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    byte[] data;
                    if (typeCode == TypeCode.Boolean)
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
            var prefix = pointMcWriteDto.Prefix.ToPrefix();
            int address = prefix.IsHexDevice()
                ? (int)Convert.ToUInt32(pointMcWriteDto.Address, 16)
                : int.Parse(pointMcWriteDto.Address);

            return PaginatedWriteing(
                pointMcWriteDto.IpAddress,
                pointMcWriteDto.Port,
                prefix,
                pointMcWriteDto.DataType,
                address,
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
            int maxRetry = 3,
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
                        case TypeCode.Boolean:
                            mcp.BatchWriteBool(
                                prefix,
                                address.ToString(),
                                value.Select(x => bool.Parse(x?.ToString())).ToArray()
                            );
                            break;
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
                    if (i == maxRetry - 1)
                        throw new Exception($"写入失败，已重试{maxRetry}次：{ex.Message}", ex);

                    MarkMcpInvalid(ipAddress, port);

                    Thread.Sleep(100);
                }
            }
            throw new Exception("重试失败");
        }

        #endregion
        private async Task<Result> WriteAsync(DevPlcPointWriteDto pointMcWriteDto)
        {
            var prefix = pointMcWriteDto.Prefix.ToPrefix();
            int address = prefix.IsHexDevice()
                ? (int)Convert.ToUInt32(pointMcWriteDto.Address, 16)
                : int.Parse(pointMcWriteDto.Address);

            return await PaginatedWriteingAsync(
                pointMcWriteDto.IpAddress,
                pointMcWriteDto.Port,
                prefix,
                pointMcWriteDto.DataType,
                address,
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
            int maxRetry = 3,
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
                        case TypeCode.Boolean:
                            await mcp.BatchWriteBoolAsync(
                                prefix,
                                address.ToString(),
                                value.Select(x => bool.Parse(x?.ToString())).ToArray()
                            );
                            break;
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
                    if (i == maxRetry - 1)
                        throw new Exception($"写入失败，已重试{maxRetry}次：{ex.Message}", ex);

                    MarkMcpInvalid(ipAddress, port);

                    await Task.Delay(retryInterval);
                }
            }
            throw new Exception("重试失败");
        }

        async Task<Result<DevPlcPointReadDto>> ReadAsync(DevPlcPointReadDto readPlcInfo)
        {
            Result<byte[]> readValue;
            try
            {
                var prefix = readPlcInfo.Prefix.ToPrefix();
                int address = prefix.IsHexDevice()
                    ? (int)Convert.ToUInt32(readPlcInfo.Address, 16)
                    : int.Parse(readPlcInfo.Address);

                readValue = await PaginatedReading(
                    readPlcInfo.IpAddress,
                    readPlcInfo.Port,
                    prefix,
                    address,
                    readPlcInfo.Length,
                    readPlcInfo.DataType
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
                    var prefix = group.Key.Prefix.ToPrefix();
                    bool isHex = prefix.IsHexDevice();

                    foreach (var groupAddre in GroupByAddress(group))
                    {
                        var list = groupAddre
                            .Select(item => new
                            {
                                Item = item,
                                Addr = isHex
                                    ? (int)Convert.ToUInt32(item.Address, 16)
                                    : int.Parse(item.Address),
                            })
                            .ToList();

                        var startAddre = list.Min(x => x.Addr);
                        var endAddressInfo = list.OrderByDescending(x => x.Addr).First();
                        var lenght =
                            endAddressInfo.Addr
                            + (
                                endAddressInfo.Item.DataType == TypeCode.String
                                    ? (int)Math.Ceiling((double)endAddressInfo.Item.Length / 2)
                                    : (endAddressInfo.Item.Length * endAddressInfo.Item.ShortOffset)
                            )
                            - startAddre;

                        var typeCode = list.All(x => x.Item.DataType == TypeCode.Boolean)
                            ? TypeCode.Boolean
                            : TypeCode.Object;

                        var readValue = await PaginatedReading(
                            group.Key.IpAddress,
                            group.Key.Port,
                            prefix,
                            startAddre,
                            lenght,
                            typeCode
                        );
                        if (readValue.IsSuccess is false)
                        {
                            byte[] bytes = new byte[lenght * 2];
                            foreach (var x in list)
                            {
                                x.Item.Value = bytes.ConvertToValues(
                                    (x.Addr - startAddre) * x.Item.ShortOffset,
                                    x.Item.DataType,
                                    x.Item.Length
                                );
                            }
                        }
                        else
                        {
                            foreach (var x in list)
                            {
                                x.Item.Value = readValue.Data.ConvertToValues(
                                    ((x.Addr - startAddre) * 2),
                                    x.Item.DataType,
                                    x.Item.Length
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
            int lenght,
            TypeCode typeCode = TypeCode.Object
        )
        {
            Result<byte[]> result = new Result<byte[]>() { IsSuccess = true };
            int currentAddress = startAddre;
            int remaining = lenght;
            List<byte> allData = new List<byte>();

            while (remaining > 0)
            {
                ushort readLen = (ushort)Math.Min(remaining, 960);

                try
                {
                    Log.Debug("读取开始。");
                    var data = await ReadWithRetry(
                        ipAddress,
                        port,
                        prefix,
                        currentAddress,
                        readLen,
                        typeCode
                    );
                    if (data.Length == 0)
                    {
                        await Task.Delay(100);
                        data = await ReadWithRetry(
                            ipAddress,
                            port,
                            prefix,
                            currentAddress,
                            readLen,
                            typeCode
                        );
                    }
                    Log.Debug("读取结束。");
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

                currentAddress += readLen;
                remaining -= readLen;
            }
            result.Data = allData.ToArray();
            return result;
        }

        async Task<byte[]> ReadWithRetry(
            string ipAddress,
            int port,
            Prefix prefix,
            int currentAddress,
            ushort readLen,
            TypeCode typeCode = TypeCode.Object,
            int maxRetry = 3,
            int retryInterval = 300
        )
        {
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    byte[] data;
                    if (typeCode == TypeCode.Boolean)
                    {
                        data = BoolArrayToByteArrayHighBit(
                            await GetMcp(ipAddress, port)
                                .BatchReadBoolAsync(prefix, currentAddress.ToString(), readLen)
                        );
                    }
                    else
                    {
                        data = await GetMcp(ipAddress, port)
                            .BatchReadByteAsync(prefix, currentAddress.ToString(), readLen);
                    }

                    return data;
                }
                catch (Exception ex)
                {
                    Log.Warning($"第{i + 1}次读取失败：{ex.Message}");

                    MarkMcpInvalid(ipAddress, port);

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

        private void MarkMcpInvalid(string ipAddress, int port)
        {
            string key = $"{ipAddress}:{port}";
            McpX oldMcp = null;

            lock (_lockObj)
            {
                if (_mcpDic.TryGetValue(key, out oldMcp))
                    _mcpDic.Remove(key);
            }

            if (oldMcp != null)
            {
                try
                {
                    oldMcp.Dispose();
                }
                catch { }
            }
        }

        byte[] BoolArrayToByteArrayHighBit(bool[] boolArray)
        {
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
