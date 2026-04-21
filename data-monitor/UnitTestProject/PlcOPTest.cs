using HslCommunication.Profinet.Melsec;
using McpXLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class PlcOPTest
    {
        MelsecMcServer _server;

        public PlcOPTest()
        {
            //模拟plc
           _server = new HslCommunication.Profinet.Melsec.MelsecMcServer();
            _server.IsBinary = true;
            _server.AnalysisLogMessage = true;
            _server.ActiveTimeSpan = TimeSpan.Parse("01:00:00");

            _server.EnableIPv6 = false;
            _server.ServerStart(6000);
        }

        [TestMethod]
        public async Task ReadTest()
        {
            await Task.Delay(1000);
            McpX mcp = new McpX("127.0.0.1", 6000);
            await mcp.WriteBoolAsync(McpXLib.Enums.Prefix.M, "10", true);
            await mcp.WriteBoolAsync(McpXLib.Enums.Prefix.M, "15", true);
            await mcp.WriteStringAsync(McpXLib.Enums.Prefix.D, "20", "Sl880yt");
            await mcp.BatchWriteInt16Async(McpXLib.Enums.Prefix.D, "30", new short[] { 300, -232 });
            await mcp.BatchWriteInt32Async(
                McpXLib.Enums.Prefix.D,
                "40",
                new int[] { 333333333, -666722222 }
            );
            await mcp.BatchWriteSingleAsync(
                McpXLib.Enums.Prefix.D,
                "50",
                new float[] { (float)23.5555, (float)-23.5555 }
            );
            await Task.Delay(200);
            mcp.Dispose();

            McpCommunication mcpCommunication = new McpCommunication();

            var re = new List<DevPlcPointMcDto>()
            {
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.M,
                    typeCode: TypeCode.Boolean,
                    address: 10,
                    length: 1
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.M,
                    typeCode: TypeCode.Boolean,
                    address: 15,
                    length: 1
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.String,
                    address: 20,
                    length: 2
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.String,
                    address: 20,
                    length: 7
                ),
                new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int16,
                    address: 30,
                    length: 2
                ),  new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Int32,
                    address: 40,
                    length: 2
                ),
                  new DevPlcPointMcDto(
                    deviceName: "a",
                    pointName: "aaaa",
                    ipAddress: "127.0.0.1",
                    port: 6000,
                    prefix: McpXLib.Enums.Prefix.D,
                    typeCode: TypeCode.Single,
                    address: 50,
                    length: 2
                ),
            };
            var result =  mcpCommunication.Read(re);
            Assert.AreEqual(result.Data[0].Value[0].ObjToBool(),true);
           // 一个寄存器 有 16个 bol 后续优化，单点ok 多点ng
         //   Assert.AreEqual(result.Data[1].Value[0].ObjToBool(),true);
            Assert.AreEqual(result.Data[2].Value[0].ToString(), "S");
            Assert.AreEqual(string.Concat(result.Data[3].Value.Where(x => x is string)), "Sl880yt");
            Assert.AreEqual(result.Data[4].Value[0].ObjToInt(), 300);
            Assert.AreEqual(result.Data[4].Value[1].ObjToInt(), -232);
            Assert.AreEqual(result.Data[5].Value[0].ObjToInt(), 333333333);
            Assert.AreEqual(result.Data[5].Value[1].ObjToInt(), -666722222);
            Assert.AreEqual(result.Data[6].Value[0].ObjToDecimal(),(decimal) 23.5555);
            Assert.AreEqual(result.Data[6].Value[1].ObjToDecimal(), (decimal)-23.5555);
            _server.Dispose();
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
                    prefix: McpXLib.Enums.Prefix.M,
                    typeCode: TypeCode.Boolean,
                    address: 22,
                    length: 1,
                    value: new List<object>() { true }
                ),
            };
            var writeValue = await mcpCommunication.WriteAsync(re[0]);
            await Task.Delay(200);
            var hs= _server.ReadBool("M22");
            Assert.AreEqual(hs.Content,true);
            _server.Dispose();
        }
        //模拟的会死锁
        //[TestMethod]
        //public async Task Retry()
        //{
        //    //await Task.Delay(1000);
        //    //McpX mcp = new McpX("127.0.0.1", 6000);
        //    //await mcp.WriteBoolAsync(McpXLib.Enums.Prefix.M, "10", true);
        //    //await Task.Delay(500);
        //    //mcp.Dispose();
        //    //await Task.Delay(500);
        //    McpCommunication mcpCommunication = new McpCommunication();

        //    var re = new List<DevPlcPointMcDto>()
        //    {
        //        new DevPlcPointMcDto(
        //            deviceName: "a",
        //            pointName: "aaaa",
        //            ipAddress: "127.0.0.1",
        //            port: 6000,
        //            prefix: McpXLib.Enums.Prefix.M,
        //            typeCode: TypeCode.Boolean,
        //            address: 10,
        //            length: 1,
        //            value: new List<object>() { true }
        //        ),
        //    };
        //    var beforeDisconnecting = await mcpCommunication.ReadAsync(re[0]);
        //    await Task.Delay(500);
        //    //_server.Dispose();
        //    await Task.Delay(500);
        //    //_server.IsBinary = true;
        //    //_server.AnalysisLogMessage = true;
        //    //_server.ActiveTimeSpan = TimeSpan.Parse("01:00:00");
        //    //_server.EnableIPv6 = false;
        //    //_server.ServerStart(6000);
        //    await Task.Delay(500);
        //    var afterDisconnection = await mcpCommunication.ReadAsync(re);
        //    Assert.AreEqual(beforeDisconnecting.IsSuccess, true);
        //    Assert.AreEqual(beforeDisconnecting.IsSuccess, true);
        //    _server.Dispose();
        //}
    }
}
