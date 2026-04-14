using McpXLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class PlcOPTest
    {
        [TestMethod]
        public async Task ReadMc()
        {
            McpCommunication mcpCommunication = new McpCommunication();

            var re = new List<DevPlcPointMcDto>()
            {
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int16,
                    address: 10,
                    length: 1
                ),
                  new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int32,
                    address: 11,
                    length: 2
                ),
                    new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Single,
                    address: 15,
                    length: 1
                )


            };
            var aa = await mcpCommunication.ReadAsync(re);
        }
    }
}
