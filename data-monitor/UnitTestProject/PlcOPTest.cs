using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McpXLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Dtos;

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
                    ipAddress: "100.100.100.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int16,
                    address: 101,
                    length: 5
                ),
                //new DevPlcPointMcDto(
                //    deviceName: "a",
                //    pointName: "aaaa",
                //    ipAddress: "127.0.0.1",
                //    port: 6000,
                //    prefix: McpXLib.Enums.Prefix.D,
                //    typeCode: TypeCode.Int32,
                //    address: 11,
                //    length: 2
                //),
                //new DevPlcPointMcDto(
                //    deviceName: "a",
                //    pointName: "aaaa",
                //    ipAddress: "127.0.0.1",
                //    port: 6000,
                //    prefix: McpXLib.Enums.Prefix.D,
                //    typeCode: TypeCode.Single,
                //    address: 15,
                //    length: 1
                //),
            };
            var aa = await mcpCommunication.ReadAsync(re);
        }

        [TestMethod]
        public async Task WirteMc()
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
                    address: 0,
                    length: 1,
                    value: new List<object>() { 444 }
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int32,
                    address: 2,
                    length: 1,
                    value: new List<object>() { 666666 }
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Single,
                    address: 4,
                    length: 1,
                    value: new List<object>() { 3.141515 }
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Single,
                    address: 6,
                    length: 3,
                    value: new List<object>() { 4.555, 9.99, 131.2323 }
                ),
            };
            var a = await mcpCommunication.WriteAsync(re[0]);
            var a1 = await mcpCommunication.WriteAsync(re[1]);
            var a2 = await mcpCommunication.WriteAsync(re[2]);
            var a3 = await mcpCommunication.WriteAsync(re[3]);
        }
    }
}
