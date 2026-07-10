using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class OmronCommunication : IPlcCommunication, IDisposable
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<string, UdpClient> _udpDic = new Dictionary<string, UdpClient>();
        private const int ReceiveTimeout = 3000;
        private byte _sid = 0;

        private static readonly Dictionary<string, byte> AreaCodes = new Dictionary<string, byte>
        {
            { "D", 0x82 }, { "DM", 0x82 },
            { "C", 0xB0 }, { "CIO", 0xB0 },
            { "W", 0xB1 }, { "WR", 0xB1 },
            { "H", 0xB2 }, { "HR", 0xB2 },
        };

        private static byte GetAreaCode(string prefix)
        {
            string key = (prefix ?? "D").ToUpper();
            if (AreaCodes.ContainsKey(key))
                return AreaCodes[key];
            return 0x82;
        }

        private static void ParseAddress(string address, out int wordOffset, out int bitOffset)
        {
            bitOffset = 0;
            var parts = address.Split('.');
            int.TryParse(parts[0], out wordOffset);
            if (parts.Length >= 2)
                int.TryParse(parts[1], out bitOffset);
        }

        public Result<DevPlcPointDto> Read(DevPlcPointDto readPlcInfo)
        {
            try
            {
                ParseAddress(readPlcInfo.Address, out int wordOff, out int bitOff);
                byte area = GetAreaCode(readPlcInfo.Prefix);
                int wordCount = readPlcInfo.Length * GetWordCount(readPlcInfo.DataType);

                byte[] resp = SendFinsCommand(readPlcInfo.IpAddress, readPlcInfo.Port,
                    0x01, 0x01, BuildReadPayload(area, wordOff, wordCount));

                if (resp == null)
                {
                    readPlcInfo.Value = Enumerable.Repeat<object>(0, readPlcInfo.Length).ToList();
                    return Result<DevPlcPointDto>.Fail("读取超时或失败");
                }

                byte[] data = ExtractData(resp);
                if (readPlcInfo.DataType == TypeCode.Boolean && bitOff > 0)
                {
                    int byteIdx = bitOff / 8;
                    int bitIdx = bitOff % 8;
                    bool val = byteIdx < data.Length && ((data[byteIdx] >> bitIdx) & 1) == 1;
                    readPlcInfo.Value = new List<object> { val };
                }
                else
                {
                    readPlcInfo.Value = ParseFinsData(data, readPlcInfo.DataType, readPlcInfo.Length);
                }

                return Result<DevPlcPointDto>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[欧姆龙]读取异常：{Ip}-{Port}-{Pre}.{Addr} {Ex}",
                    readPlcInfo.IpAddress, readPlcInfo.Port, readPlcInfo.Prefix, readPlcInfo.Address, ex);
                readPlcInfo.Value = Enumerable.Repeat<object>(0, readPlcInfo.Length).ToList();
                return Result<DevPlcPointDto>.Fail(ex.Message);
            }
        }

        public Result<List<DevPlcPointDto>> Read(List<DevPlcPointDto> readPlcInfo)
        {
            try
            {
                var items = readPlcInfo.Select(x =>
                {
                    ParseAddress(x.Address, out int wordOff, out _);
                    return new { Dto = x, WordOff = wordOff };
                }).ToList();

                foreach (var group in items.GroupBy(x => new { x.Dto.IpAddress, x.Dto.Port }))
                {
                    byte area = GetAreaCode(group.First().Dto.Prefix);
                    var sorted = group.OrderBy(x => x.WordOff).ToList();
                    int startAddr = sorted.Min(x => x.WordOff);
                    var last = sorted.Last();
                    int endAddr = last.WordOff + last.Dto.Length * GetWordCount(last.Dto.DataType);
                    int wordCount = endAddr - startAddr;

                    byte[] resp = SendFinsCommand(group.Key.IpAddress, group.Key.Port,
                        0x01, 0x01, BuildReadPayload(area, startAddr, wordCount));

                    byte[] data = resp != null ? ExtractData(resp) : null;

                    foreach (var item in sorted)
                    {
                        if (data != null)
                        {
                            int offset = (item.WordOff - startAddr) * 2;
                            item.Dto.Value = ParseFinsData(data, offset,
                                item.Dto.DataType, item.Dto.Length);
                        }
                        else
                        {
                            item.Dto.Value = Enumerable.Repeat<object>(0, item.Dto.Length).ToList();
                        }
                    }
                }

                return Result<List<DevPlcPointDto>>.Success(readPlcInfo);
            }
            catch (Exception ex)
            {
                Log.Warning("[欧姆龙]批量读取异常：{Ex}", ex.Message);
                return Result<List<DevPlcPointDto>>.Fail(ex.Message);
            }
        }

        public Result Write(DevPlcPointDto devPlcPointDto)
        {
            try
            {
                ParseAddress(devPlcPointDto.Address, out int wordOff, out _);
                byte area = GetAreaCode(devPlcPointDto.Prefix);

                byte[] payload = BuildWritePayload(area, wordOff,
                    devPlcPointDto.Value, devPlcPointDto.DataType);

                byte[] resp = SendFinsCommand(devPlcPointDto.IpAddress, devPlcPointDto.Port,
                    0x01, 0x02, payload);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Warning("[欧姆龙]写入异常：{Ip}-{Port}-{Pre}.{Addr} {Ex}",
                    devPlcPointDto.IpAddress, devPlcPointDto.Port,
                    devPlcPointDto.Prefix, devPlcPointDto.Address, ex);
                return Result.Fail(ex.Message);
            }
        }

        public Result Write(List<DevPlcPointDto> pointMcWriteDto)
        {
            throw new NotImplementedException();
        }

        #region FINS 协议

        private byte[] SendFinsCommand(string ip, int port, byte cmd1, byte cmd2, byte[] payload)
        {
            UdpClient udp = GetUdp(ip, port);
            _sid++;

            byte[] header = new byte[] {
                0x80, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x0B, 0x00, _sid
            };

            byte[] request = new byte[header.Length + 2 + payload.Length];
            Buffer.BlockCopy(header, 0, request, 0, header.Length);
            request[header.Length] = cmd1;
            request[header.Length + 1] = cmd2;
            Buffer.BlockCopy(payload, 0, request, header.Length + 2, payload.Length);

            try
            {
                udp.Send(request, request.Length);
                var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                byte[] recv = udp.Receive(ref ep);

                if (recv.Length < 14) return null;
                int endCode = (recv[12] << 8) | recv[13];
                if (endCode != 0) return null;

                byte[] result = new byte[recv.Length - 14];
                Buffer.BlockCopy(recv, 14, result, 0, result.Length);
                return result;
            }
            catch
            {
                MarkInvalid(ip, port);
                return null;
            }
        }

        private static byte[] BuildReadPayload(byte area, int startWord, int wordCount)
        {
            byte[] data = new byte[5];
            data[0] = area;
            data[1] = (byte)(startWord >> 8);
            data[2] = (byte)(startWord & 0xFF);
            data[3] = (byte)(wordCount >> 8);
            data[4] = (byte)(wordCount & 0xFF);
            return data;
        }

        private static byte[] BuildWritePayload(byte area, int startWord,
            List<object> values, TypeCode type)
        {
            byte[] wordData = ValuesToFinsBytes(values, type);
            int wordCount = wordData.Length / 2;

            byte[] data = new byte[5 + wordData.Length];
            data[0] = area;
            data[1] = (byte)(startWord >> 8);
            data[2] = (byte)(startWord & 0xFF);
            data[3] = (byte)(wordCount >> 8);
            data[4] = (byte)(wordCount & 0xFF);
            Buffer.BlockCopy(wordData, 0, data, 5, wordData.Length);
            return data;
        }

        private static byte[] ExtractData(byte[] finsResponse)
        {
            return finsResponse;
        }

        private static int GetWordCount(TypeCode type)
        {
            int bytes = type.GetTypeByteLength();
            return (bytes + 1) / 2;
        }

        private static List<object> ParseFinsData(byte[] data, TypeCode type, int count)
        {
            return ParseFinsData(data, 0, type, count);
        }

        private static List<object> ParseFinsData(byte[] data, int startByte, TypeCode type, int count)
        {
            var result = new List<object>();
            int wordLen = GetWordCount(type) * 2;

            for (int i = 0; i < count; i++)
            {
                int index = startByte + i * wordLen;
                switch (type)
                {
                    case TypeCode.Boolean:
                        result.Add((data[index] & 1) == 1);
                        break;
                    case TypeCode.Byte:
                        result.Add(data[index]);
                        break;
                    case TypeCode.Int16:
                        result.Add((short)((data[index] << 8) | data[index + 1]));
                        break;
                    case TypeCode.UInt16:
                        result.Add((ushort)((data[index] << 8) | data[index + 1]));
                        break;
                    case TypeCode.Int32:
                        result.Add((int)((data[index] << 24) | (data[index + 1] << 16)
                            | (data[index + 2] << 8) | data[index + 3]));
                        break;
                    case TypeCode.UInt32:
                        result.Add((uint)((data[index] << 24) | (data[index + 1] << 16)
                            | (data[index + 2] << 8) | data[index + 3]));
                        break;
                    case TypeCode.Single:
                        byte[] fbytes = { data[index + 1], data[index], data[index + 3], data[index + 2] };
                        result.Add(BitConverter.ToSingle(fbytes, 0));
                        break;
                    case TypeCode.Double:
                        byte[] dbytes = {
                            data[index + 1], data[index],
                            data[index + 3], data[index + 2],
                            data[index + 5], data[index + 4],
                            data[index + 7], data[index + 6]
                        };
                        result.Add(BitConverter.ToDouble(dbytes, 0));
                        break;
                    default:
                        result.Add((char)data[index]);
                        break;
                }
            }
            return result;
        }

        private static byte[] ValuesToFinsBytes(List<object> values, TypeCode type)
        {
            int wordLen = GetWordCount(type);
            byte[] result = new byte[values.Count * wordLen * 2];

            for (int i = 0; i < values.Count; i++)
            {
                int index = i * wordLen * 2;
                switch (type)
                {
                    case TypeCode.Int16:
                        short s = Convert.ToInt16(values[i]);
                        result[index] = (byte)(s >> 8);
                        result[index + 1] = (byte)(s & 0xFF);
                        break;
                    case TypeCode.UInt16:
                        ushort us = Convert.ToUInt16(values[i]);
                        result[index] = (byte)(us >> 8);
                        result[index + 1] = (byte)(us & 0xFF);
                        break;
                    case TypeCode.Int32:
                        int iv = Convert.ToInt32(values[i]);
                        result[index] = (byte)(iv >> 24);
                        result[index + 1] = (byte)((iv >> 16) & 0xFF);
                        result[index + 2] = (byte)((iv >> 8) & 0xFF);
                        result[index + 3] = (byte)(iv & 0xFF);
                        break;
                    case TypeCode.UInt32:
                        uint uiv = Convert.ToUInt32(values[i]);
                        result[index] = (byte)(uiv >> 24);
                        result[index + 1] = (byte)((uiv >> 16) & 0xFF);
                        result[index + 2] = (byte)((uiv >> 8) & 0xFF);
                        result[index + 3] = (byte)(uiv & 0xFF);
                        break;
                    case TypeCode.Single:
                        byte[] fbytes = BitConverter.GetBytes(Convert.ToSingle(values[i]));
                        result[index] = fbytes[1];
                        result[index + 1] = fbytes[0];
                        result[index + 2] = fbytes[3];
                        result[index + 3] = fbytes[2];
                        break;
                    case TypeCode.Double:
                        byte[] dbytes = BitConverter.GetBytes(Convert.ToDouble(values[i]));
                        result[index] = dbytes[1]; result[index + 1] = dbytes[0];
                        result[index + 2] = dbytes[3]; result[index + 3] = dbytes[2];
                        result[index + 4] = dbytes[5]; result[index + 5] = dbytes[4];
                        result[index + 6] = dbytes[7]; result[index + 7] = dbytes[6];
                        break;
                    default:
                        result[index] = (byte)(values[i] != null ? Convert.ToByte(values[i]) : 0);
                        break;
                }
            }
            return result;
        }

        #endregion

        #region 连接管理

        private UdpClient GetUdp(string ip, int port)
        {
            string key = ip + ":" + port;
            lock (_lockObj)
            {
                if (_udpDic.ContainsKey(key))
                    return _udpDic[key];
                var udp = new UdpClient();
                udp.Client.SendTimeout = 2000;
                udp.Client.ReceiveTimeout = ReceiveTimeout;
                udp.Connect(ip, port);
                _udpDic[key] = udp;
                return udp;
            }
        }

        private void MarkInvalid(string ip, int port)
        {
            string key = ip + ":" + port;
            UdpClient old = null;
            lock (_lockObj)
            {
                if (_udpDic.TryGetValue(key, out old))
                    _udpDic.Remove(key);
            }
            if (old != null)
                try { old.Close(); } catch { }
        }

        public void Dispose()
        {
            List<UdpClient> list;
            lock (_lockObj)
            {
                list = _udpDic.Values.ToList();
                _udpDic.Clear();
            }
            foreach (var u in list)
                try { u.Close(); } catch { }
        }

        #endregion
    }
}
