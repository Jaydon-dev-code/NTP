using Org.BouncyCastle.Utilities;
using S7.Net;
using S7.Net.Types;
using Serilog;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class S7Communication : IPlcCommunication, IDisposable
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<string, Plc> _plcDic = new Dictionary<string, Plc>();
        private const int MaxReadBytes = 254;

        private static void ParseAddress(string address, out int byteOffset, out int bitOffset)
        {
            bitOffset = 0;
            int start = 0;
            while (start < address.Length && !char.IsDigit(address[start]))
                start++;
            address = address.Substring(start);
            var parts = address.Split('.');
            int.TryParse(parts[0], out byteOffset);
            if (parts.Length >= 2)
                int.TryParse(parts[1], out bitOffset);
        }

        public Result<DevPlcPointDto> Read(DevPlcPointDto readPlcInfo)
        {
            try
            {
                int db = int.Parse(readPlcInfo.Prefix.Replace("DB", ""));
                ParseAddress(readPlcInfo.Address, out int byteOffset, out int bitOffset);
                var plc = GetPlc(readPlcInfo.IpAddress, readPlcInfo.Port);

                if (readPlcInfo.DataType == TypeCode.Boolean && bitOffset > 0)
                {
                    byte[] raw = ReadWithRetry(plc, db, byteOffset, 1);
                    bool val = raw != null && ((raw[0] >> bitOffset) & 1) == 1;
                    readPlcInfo.Value = new List<object> { val };
                }
                else
                {
                    int byteLen = readPlcInfo.Length * readPlcInfo.DataType.GetTypeByteLength();
                    byte[] raw = ReadWithRetry(plc, db, byteOffset, byteLen);
                    if (raw == null)
                        readPlcInfo.Value = Enumerable
                            .Repeat<object>(0, readPlcInfo.Length)
                            .ToList();
                    else
                        readPlcInfo.Value = ConvertBytes(
                            raw,
                            readPlcInfo.DataType,
                            readPlcInfo.Length
                        );
                }
                return Result<DevPlcPointDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning(
                    "[S7]读取异常：{Ip}-{Port}-DB{Db}.{Addr} {Ex}",
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
                    .Select(x =>
                    {
                        int db = int.Parse(x.Prefix);
                        ParseAddress(x.Address, out int byteOff, out _);
                        return new
                        {
                            Dto = x,
                            Db = db,
                            ByteOff = byteOff,
                        };
                    })
                    .ToList();

                foreach (
                    var group in items.GroupBy(x => new
                    {
                        x.Dto.IpAddress,
                        x.Dto.Port,
                        x.Db,
                    })
                )
                {
                    var sorted = group.OrderBy(x => x.ByteOff).ToList();
                    int startAddr = sorted.Min(x => x.ByteOff);
                    var last = sorted.Last();
                    int endAddr =
                        last.ByteOff + last.Dto.Length * last.Dto.DataType.GetTypeByteLength();
                    int length = endAddr - startAddr;

                    var plc = GetPlc(group.Key.IpAddress, group.Key.Port);
                    byte[] raw = ReadWithRetry(plc, group.Key.Db, startAddr, length);

                    foreach (var item in sorted)
                    {
                        int offset = item.ByteOff - startAddr;
                        if (raw != null)
                            item.Dto.Value = ConvertBytes(
                                raw,
                                offset,
                                item.Dto.DataType,
                                item.Dto.Length
                            );
                        else
                            item.Dto.Value = Enumerable.Repeat<object>(0, item.Dto.Length).ToList();
                    }
                }
                return Result<List<DevPlcPointDto>>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[S7]批量读取异常：{Ex}", ex.Message);
                return Result<List<DevPlcPointDto>>.Fail(ex.Message);
            }
        }

        public Result Write(DevPlcPointDto devPlcPointDto)
        {
            try
            {
                int db = int.Parse(devPlcPointDto.Prefix);
                ParseAddress(devPlcPointDto.Address, out int byteOffset, out int bitOffset);
                var plc = GetPlc(devPlcPointDto.IpAddress, devPlcPointDto.Port);

                if (devPlcPointDto.DataType == TypeCode.Boolean)
                {
                    bool val =
                        devPlcPointDto.Value != null
                        && devPlcPointDto.Value[0].ToString() == "True";
                    if (bitOffset > 0)
                        plc.WriteBit(DataType.DataBlock, db, byteOffset, bitOffset, val);
                    else
                        plc.Write(DataType.DataBlock, db, byteOffset, val, 0);
                }
                else
                {
                    int byteLen = devPlcPointDto.DataType.GetTypeByteLength();
                    int offset = byteOffset;
                    foreach (object val in devPlcPointDto.Value)
                    {
                        byte[] bytes = ToS7Bytes(val, devPlcPointDto.DataType);
                        plc.WriteBytes(DataType.DataBlock, db, offset, bytes);
                        offset += byteLen;
                    }
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Warning(
                    "[S7]写入异常：{Ip}-{Port}-DB{Db}.{Addr} {Ex}",
                    devPlcPointDto.IpAddress,
                    devPlcPointDto.Port,
                    devPlcPointDto.Prefix,
                    devPlcPointDto.Address,
                    ex
                );
                return Result.Fail(ex.Message);
            }
        }

        public Result Write(List<DevPlcPointDto> pointMcWriteDto)
        {
            throw new NotImplementedException();
        }

        private byte[] ReadWithRetry(Plc plc, int db, int addr, int length, int retry = 3)
        {
            for (int i = 0; i < retry; i++)
            {
                try
                {
                    var data = new List<byte>();
                    int remaining = length;
                    int cur = addr;
                    while (remaining > 0)
                    {
                        int len = Math.Min(remaining, MaxReadBytes);
                        data.AddRange(plc.ReadBytes(DataType.DataBlock, db, cur, len));
                        cur += len;
                        remaining -= len;
                    }
                    return data.ToArray();
                }
                catch
                {
                    if (i == retry - 1)
                        return null;
                    Thread.Sleep(200);
                }
            }
            return null;
        }

        private Plc GetPlc(string ip, int port)
        {
            string key = ip + ":" + port;
            lock (_lockObj)
            {
                if (_plcDic.ContainsKey(key))
                    return _plcDic[key];
                var plc = new Plc(CpuType.S71200, ip, port, 0, 1);
                plc.Open();
                _plcDic[key] = plc;
                return plc;
            }
        }

        private static List<object> ConvertBytes(byte[] buffer, TypeCode type, int count)
        {
            return ConvertBytes(buffer, 0, type, count);
        }

        private static List<object> ConvertBytes(byte[] arr, int start, TypeCode type, int count)
        {
            var result = new List<object>();
            int byteLen = type.GetTypeByteLength();
            //if (TypeCode.String==type)
            //{
            //  return result.Add(S7String.FromByteArray(arr))  ;
            //}


          
                for (int i = 0; i < count; i++)
            {
                int index = start + i * byteLen;
                var buffer = arr.Skip(index).Take(byteLen).ToArray();
                switch (type)
                {
                    case TypeCode.Boolean:
                        result.Add((buffer[index] & 1) == 1);
                        break;
                    case TypeCode.Byte:
                        result.Add(buffer[index]);
                        break;
                    case TypeCode.Int16:

                        result.Add(S7.Net.Types.Int.FromByteArray(buffer));
                        break;
                    case TypeCode.UInt16:
                        result.Add((ushort)S7.Net.Types.Int.FromByteArray(buffer));
                        break;
                    case TypeCode.Int32:
                        result.Add(DInt.FromByteArray(buffer));
                        break;
                    case TypeCode.UInt32:
                        result.Add((uint)DInt.FromByteArray(buffer));
                        break;
                    case TypeCode.Single:
                        result.Add(Real.FromByteArray(buffer));
                        break;
                    case TypeCode.Double:
                        result.Add(BitConverter.ToDouble(buffer, index));
                        break;
                 
                    default:
                        result.Add((char)buffer[index]);
                        break;
                }
            }
            return result;
        }

        private static byte[] ToS7Bytes(object value, TypeCode type)
        {
            switch (type)
            {
                case TypeCode.Byte:
                    return new byte[] { Convert.ToByte(value) };
                case TypeCode.Int16:
                    return BitConverter.GetBytes(Convert.ToInt16(value));
                case TypeCode.UInt16:
                    return BitConverter.GetBytes(Convert.ToUInt16(value));
                case TypeCode.Int32:
                    return BitConverter.GetBytes(Convert.ToInt32(value));
                case TypeCode.UInt32:
                    return BitConverter.GetBytes(Convert.ToUInt32(value));
                case TypeCode.Single:
                    return BitConverter.GetBytes(Convert.ToSingle(value));
                case TypeCode.Double:
                    return BitConverter.GetBytes(Convert.ToDouble(value));
                default:
                    return new byte[] { Convert.ToByte(value) };
            }
        }

        public void Dispose()
        {
            List<Plc> list;
            lock (_lockObj)
            {
                list = _plcDic.Values.ToList();
                _plcDic.Clear();
            }
            foreach (var p in list)
                try
                {
                    p.Close();
                }
                catch { }
        }
    }
}
