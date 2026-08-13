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
        public void Read_Length240()
        {
            short[] values = Enumerable.Range(1, 240).Select(i => (short)i).ToArray();
            _server.Write("D100", values);

            var re = _plcCommunication.Read(GetDevPlcPointDto("100", TypeCode.Int16, "D", 240));

            Assert.IsTrue(re.IsSuccess);
            Assert.AreEqual(240, re.Data.Value.Count);
            Assert.AreEqual((short)1, short.Parse(re.Data.Value[0].ToString()));
            Assert.AreEqual((short)120, short.Parse(re.Data.Value[119].ToString()));
            Assert.AreEqual((short)121, short.Parse(re.Data.Value[120].ToString()));
            Assert.AreEqual((short)240, short.Parse(re.Data.Value[239].ToString()));
        }

        [TestMethod]
        public void Read_Length_CrossFrameBoundary()
        {
            short[] values = Enumerable.Range(1, 500).Select(i => (short)i).ToArray();
            _server.Write("D100", values);

            var re = _plcCommunication.Read(GetDevPlcPointDto("100", TypeCode.Int16, "D", 500));

            Assert.IsTrue(re.IsSuccess);
            Assert.AreEqual(500, re.Data.Value.Count);
            Assert.AreEqual((short)1, short.Parse(re.Data.Value[0].ToString()));
            Assert.AreEqual((short)239, short.Parse(re.Data.Value[238].ToString()));
            Assert.AreEqual((short)240, short.Parse(re.Data.Value[239].ToString()));
            Assert.AreEqual((short)241, short.Parse(re.Data.Value[240].ToString()));
            Assert.AreEqual((short)480, short.Parse(re.Data.Value[479].ToString()));
            Assert.AreEqual((short)500, short.Parse(re.Data.Value[499].ToString()));
        }

        [TestMethod]
        public void Read_Length241()
        {
            short[] values = Enumerable.Range(1, 241).Select(i => (short)i).ToArray();
            _server.Write("D100", values);

            var re = _plcCommunication.Read(GetDevPlcPointDto("100", TypeCode.Int16, "D", 241));

            Assert.IsTrue(re.IsSuccess);
            Assert.AreEqual(241, re.Data.Value.Count);
            Assert.AreEqual((short)240, short.Parse(re.Data.Value[239].ToString()));
            Assert.AreEqual((short)241, short.Parse(re.Data.Value[240].ToString()));
        }

        [TestMethod]
        public void Read_Length241GroupBy()
        {
            short[] values = Enumerable.Range(1, 800).Select(i => (short)i).ToArray();
            _server.Write("D100", values);

            var re = _plcCommunication.Read(new List<DevPlcPointDto> { GetDevPlcPointDto("100", TypeCode.Int16, "D", 1), GetDevPlcPointDto("150", TypeCode.Int16, "D", 1), GetDevPlcPointDto("200", TypeCode.Int16, "D", 1), GetDevPlcPointDto("230", TypeCode.Int16, "D", 1), GetDevPlcPointDto("600", TypeCode.Int16, "D", 1) });

            Assert.IsTrue(re.IsSuccess);
            Assert.AreEqual((short)1, short.Parse(re.Data[0].Value[0].ToString()));
            Assert.AreEqual((short)51, short.Parse(re.Data[1].Value[0].ToString()));
            Assert.AreEqual((short)101, short.Parse(re.Data[2].Value[0].ToString()));
            Assert.AreEqual((short)131, short.Parse(re.Data[3].Value[0].ToString()));
            Assert.AreEqual((short)501, short.Parse(re.Data[4].Value[0].ToString()));
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

        [TestCleanup]
        public void Cleanup()
        {
            _server?.ServerClose();
            _server = null;
        }
    }
}
