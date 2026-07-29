using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McpXLib.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;

namespace UnitTestProject.PLC
{
    [TestClass]
    public class McpCommunicationTest
    {
        HslCommunication.Profinet.Melsec.MelsecMcServer _server;
        IPlcCommunication _plcCommunication;

        public McpCommunicationTest()
        {
            InitPlc();
            _plcCommunication = new McpCommunication();
        }

        [TestMethod]
        public void Read()
        {
            _server.Write("D100", (short)33);
            _server.Write("D101", 6666666);
            _server.Write("D103", (float)33.333);
            var re = _plcCommunication.Read(
                new List<DevPlcPointDto>()
                {
                    GetDevPlcPointDto("100", TypeCode.Int16, "D"),
                    GetDevPlcPointDto("101", TypeCode.Int32, "D"),
                    GetDevPlcPointDto("103", TypeCode.Single, "D"),
                }
            );

            Assert.AreEqual((short)33, short.Parse(re.Data[0].Value[0].ToString()));
            Assert.AreEqual(6666666, int.Parse(re.Data[1].Value[0].ToString()));
            Assert.AreEqual((float)33.333, float.Parse(re.Data[2].Value[0].ToString()));
        }

        [TestMethod]
        public void Read_Bacth()
        {
            _server.Write("D100", new int[] { 6666666, 7777777, 888888888 });

            var re = _plcCommunication.Read(GetDevPlcPointDto("100", TypeCode.Int32, "D", 3));

            Assert.AreEqual(6666666, int.Parse(re.Data.Value[0].ToString()));
            Assert.AreEqual(7777777, int.Parse(re.Data.Value[1].ToString()));
            Assert.AreEqual(888888888, int.Parse(re.Data.Value[2].ToString()));
        }

        [TestMethod]
        public void Read_X16Address_Bacth()
        {
            _server.Write("XA", true);
            _server.Write("XC", true);
            _server.Write("X10", true);

            var re = _plcCommunication.Read(
                new List<DevPlcPointDto>()
                {
                    GetDevPlcPointDto("A", TypeCode.Boolean, "X"),
                    GetDevPlcPointDto("C", TypeCode.Boolean, "X"),
                    GetDevPlcPointDto("10", TypeCode.Boolean, "X"),
                }
            );

            Assert.AreEqual(true, bool.Parse(re.Data[0].Value[0].ToString()));
            Assert.AreEqual(true, bool.Parse(re.Data[1].Value[0].ToString()));
            Assert.AreEqual(true, bool.Parse(re.Data[2].Value[0].ToString()));
        }

        [TestMethod]
        public void Read_X16Address()
        {
            _server.Write("XA", true);

            var re = _plcCommunication.Read(GetDevPlcPointDto("A", TypeCode.Boolean, "X"));

            Assert.AreEqual(true, bool.Parse(re.Data.Value[0].ToString()));
            _server.Write("XC", true);
            var reLen = _plcCommunication.Read(GetDevPlcPointDto("A", TypeCode.Boolean, "X", 3));
            Assert.AreEqual(true, bool.Parse(reLen.Data.Value[0].ToString()));
            Assert.AreEqual(false, (reLen.Data.Value[1].ObjToBool()));
            Assert.AreEqual(true, bool.Parse(reLen.Data.Value[2].ToString()));
        }

        DevPlcPointDto GetDevPlcPointDto(
            string address,
            TypeCode dataType,
            string prefix,
            int length = 1,
            List<object> value = null
        )
        {
            return new DevPlcPointDto()
            {
                IpAddress = "127.0.0.1",
                Port = 6000,
                Address = address,
                DataType = dataType,
                Prefix = prefix,
                Length = length,
                Value = value,
            };
        }

        private void InitPlc()
        {
            _server = new HslCommunication.Profinet.Melsec.MelsecMcServer();
            _server.IsBinary = true;
            _server.AnalysisLogMessage = true;
            _server.ActiveTimeSpan = TimeSpan.Parse("01:00:00");

            _server.EnableIPv6 = false;
            _server.ServerStart(6000);
        }
    }
}
