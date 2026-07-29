using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using McpXLib.Enums;
using Microsoft.Owin.BuilderProperties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Domain.Mc1E;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;

namespace UnitTestProject.PLC
{
    /// <summary>
    /// MC QNA 1E 协议通信单元测试
    /// <para>覆盖 Mc1ECommunication / Mc1EWirte / Mc1ERead 的帧构建、地址编码、值转换等核心逻辑。</para>
    /// </summary>
    [TestClass]
    public class Mc1ECommunicationTest
    {
        HslCommunication.Profinet.Melsec.MelsecA1EServer _server;
        IPlcCommunication _plcCommunication;

        public Mc1ECommunicationTest()
        {
            InitPlc();
            _plcCommunication = new Mc1ECommunication();
        }

        [TestMethod]
        public void Read_X()
        {
            _server.Write("X11", true);
            var x11Value = _plcCommunication.Read(GetDevPlcPointDto("11", TypeCode.Boolean, "X"));
            Assert.AreEqual(true, x11Value.Data.Value[0].ObjToBool());
            _server.Write("X13", true);
            var x11_13Value = _plcCommunication.Read(
                new List<DevPlcPointDto>()
                {
                    GetDevPlcPointDto("11", TypeCode.Boolean, "X"),
                    GetDevPlcPointDto("12", TypeCode.Boolean, "X"),
                    GetDevPlcPointDto("13", TypeCode.Boolean, "X"),
                }
            );
            Assert.AreEqual(true, x11_13Value.Data[0].Value[0].ObjToBool());
            Assert.AreEqual(false, x11_13Value.Data[1].Value[0].ObjToBool());
            Assert.AreEqual(true, x11_13Value.Data[2].Value[0].ObjToBool());
        }

        [TestMethod]
        public void Read_String()
        {
            var str = "HelloWord";
            _server.Write("D200", str);
            var d200Value = _plcCommunication.Read(
                GetDevPlcPointDto("200", TypeCode.String, "D", 10)
            );
            Assert.AreEqual(str, d200Value.Data.Value[0].ToString());

            var d200yibanValue = _plcCommunication.Read(
                GetDevPlcPointDto("200", TypeCode.String, "D", 5)
            );
            Assert.AreEqual("Hello", d200yibanValue.Data.Value[0].ToString());
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

        [TestMethod]
        public void Write_String()
        {
            var str = "HelloWord";

            var d200Value = _plcCommunication.Write(
                GetDevPlcPointDto("200", TypeCode.String, "D", 10, new List<object>() { str })
            );
            var readVal = _server.ReadString("D200", 5);

            Assert.AreEqual(str, readVal.Content.Replace('\0', ' ').Trim());
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
            _server = new HslCommunication.Profinet.Melsec.MelsecA1EServer();
            _server.IsBinary = true;
            _server.AnalysisLogMessage = true;
            _server.ActiveTimeSpan = TimeSpan.Parse("01:00:00");
            _server.EnableIPv6 = false;
            _server.ServerStart(6000);
        }
    }
}
