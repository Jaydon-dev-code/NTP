using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McpXLib;
using McpXLib.Enums;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Dtos;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public class McpCommunication
    {
        Dictionary<string, McpX> _mcpDic;

        public McpCommunication()
        {
            _mcpDic = new Dictionary<string, McpX>();
        }

        McpX GetMcp(string ipAddress, int port)
        {
            if (_mcpDic.TryGetValue(ipAddress + port, out McpX mcp))
            {
                return mcp;
            }
            else
            {
                var mcpAdd = new McpX(ipAddress, port);
                _mcpDic.Add(ipAddress + port, mcpAdd);
                return mcpAdd;
            }
        }

        public async Task<List<DevPlcPointMcDto>> ReadAsync(List<DevPlcPointMcDto> lineReadPlcInfo)
        {
            var re = await ReadAsync(
                lineReadPlcInfo
                    .Select(x => new DevPlcPointMcReadDto(x,x.DataType.GetTypeOfShortOffset()))
                    .ToList()
            );
            for (int i = 0; i < lineReadPlcInfo.Count; i++)
            {
                lineReadPlcInfo[i].Value = re[i].Value;
            }
            return lineReadPlcInfo;
        }

        async Task<List<DevPlcPointMcReadDto>> ReadAsync(List<DevPlcPointMcReadDto> lineReadPlcInfo)
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
                    var lenght = endAddressInfo.Address+(endAddressInfo.Length * endAddressInfo.ShortOffset) - startAddre;
                    var mcp = GetMcp(group.Key.IpAddress, group.Key.Port);
                    var readValue = await PaginatedReading(
                        group.Key.Prefix,
                        startAddre,
                        lenght,
                        mcp
                    );
                    foreach (DevPlcPointMcReadDto item in group)
                    {
                        item.Value = readValue.ConvertToValues(
                           ( item.Address- startAddre)*item.ShortOffset,
                            item.DataType,
                            item.Length
                        );
                    }
                }
                return lineReadPlcInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<byte[]> PaginatedReading(
            Prefix prefix,
            int startAddre,
            int lenght,
            McpX mcp
        )
        {
            // 开始地址
            long currentAddress = startAddre;
            // 剩余长度
            long remaining = lenght;
            // 存储所有读取结果
            List<byte> allData = new List<byte>();

            // 自动循环分页读取
            while (remaining > 0)
            {
                // 本次读取长度：最多 65535
                ushort readLen = (ushort)Math.Min(remaining, ushort.MaxValue);

                try
                {
                    // 调用你的读取方法
                    var data = await mcp.BatchReadByteAsync(
                        prefix,
                        currentAddress.ToString(),
                        readLen
                    );

                    // 把读到的数据加入总结果
                    allData.AddRange(data);
                }
                catch (Exception ex)
                {
                    allData.AddRange(new byte[readLen]);

                    throw;
                }

                // 偏移地址
                currentAddress += readLen;
                // 减少剩余长度
                remaining -= readLen;
            }

            // 最终所有数据在这里
            return allData.ToArray();
        }

        //    void Read(List<PlcPointMcReadDto> variables)
        //    {
        //        // 2. 自动计算每个变量的Byte长度 + 连续起始索引（关键）
        //        int currentIndex = 0;
        //        foreach (var variable in variables)
        //        {
        //            variable.ByteLength = variable.DataType.GetTypeByteLength();
        //            variable.ByteStartIndex = currentIndex;
        //            currentIndex += variable.ByteLength;
        //        }

        //        // 3. 【最优】一次性读取总长度的Byte数组
        //        // 这里替换成你的MCP库读取Byte方法
        //        byte[] plcRawData = ReadPlcBytes("DB1", 0, currentIndex);

        //        // 4. 批量解析：Byte数组 → 对应数据类型
        //        foreach (var variable in variables)
        //        {
        //            variable.Value = PlcByteConverter.ConvertToValue(
        //                plcRawData,
        //                variable.ByteStartIndex,
        //                variable.DataType
        //            );
        //        }

        //    //McpX GetMcp(McpLinkInfo mcpLinkInfo)
        //    //{
        //    //    if (_mcpDic.TryGetValue($"{mcpLinkInfo.Ip}{mcpLinkInfo.Port}", out McpX re))
        //    //    {
        //    //        return re;
        //    //    }
        //    //    else
        //    //    {
        //    //        var mcpx = new McpX(mcpLinkInfo.Ip, mcpLinkInfo.Port, isAscii: mcpLinkInfo.IsAscii);
        //    //        _mcpDic.Add($"{mcpLinkInfo.Ip}{mcpLinkInfo.Port}", mcpx);
        //    //        mcpx.Read<byte>
        //    //        return mcpx;
        //    //    }
        //    //}
        //    //public void Read()
        //    //{
        //    //    using (var mcpx = new McpX("127.0.0.1", 6000))
        //    //    {
        //    //        // Read 7000 points starting from M0

        //    //        // Read 7000 words starting from D1000
        //    //        short[] dArr = mcpx.BatchRead<short>(Prefix.D, "120", 1);

        //    //        // Write 1234 to D0 and 5678 to D1 as signed 32-bit integers
        //    //        mcpx.BatchWrite<int>(Prefix.D, "120", [1234]);
        //    //    }
        //    //}
        //}
    }
}
