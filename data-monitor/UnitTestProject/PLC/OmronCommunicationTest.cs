using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitTestProject.PLC
{
    /// <summary>
    /// 欧姆龙 FINS 协议通信单元测试（通过反射测试私有方法）
    /// </summary>
    [TestClass]
    public class OmronCommunicationTest
    {
        private static readonly Type _omronType = typeof(OmronCommunication);

        #region GetAreaCode 区域码映射测试

        [TestMethod]
        public void GetAreaCode_D_Returns0x82()
        {
            var result = InvokeGetAreaCode("D");
            Assert.AreEqual((byte)0x82, result);
        }

        [TestMethod]
        public void GetAreaCode_DM_Returns0x82()
        {
            var result = InvokeGetAreaCode("DM");
            Assert.AreEqual((byte)0x82, result);
        }

        [TestMethod]
        public void GetAreaCode_C_Returns0xB0()
        {
            var result = InvokeGetAreaCode("C");
            Assert.AreEqual((byte)0xB0, result);
        }

        [TestMethod]
        public void GetAreaCode_CIO_Returns0xB0()
        {
            var result = InvokeGetAreaCode("CIO");
            Assert.AreEqual((byte)0xB0, result);
        }

        [TestMethod]
        public void GetAreaCode_W_Returns0xB1()
        {
            var result = InvokeGetAreaCode("W");
            Assert.AreEqual((byte)0xB1, result);
        }

        [TestMethod]
        public void GetAreaCode_WR_Returns0xB1()
        {
            var result = InvokeGetAreaCode("WR");
            Assert.AreEqual((byte)0xB1, result);
        }

        [TestMethod]
        public void GetAreaCode_H_Returns0xB2()
        {
            var result = InvokeGetAreaCode("H");
            Assert.AreEqual((byte)0xB2, result);
        }

        [TestMethod]
        public void GetAreaCode_HR_Returns0xB2()
        {
            var result = InvokeGetAreaCode("HR");
            Assert.AreEqual((byte)0xB2, result);
        }

        [TestMethod]
        public void GetAreaCode_Null_ReturnsDefault0x82()
        {
            var result = InvokeGetAreaCode(null);
            Assert.AreEqual((byte)0x82, result);
        }

        [TestMethod]
        public void GetAreaCode_Lowercase_ReturnsCorrectCode()
        {
            var result = InvokeGetAreaCode("d");
            Assert.AreEqual((byte)0x82, result);
        }

        [TestMethod]
        public void GetAreaCode_Unknown_ReturnsDefault0x82()
        {
            var result = InvokeGetAreaCode("X");
            Assert.AreEqual((byte)0x82, result);
        }

        #endregion

        #region GetWordCount 类型字计数测试

        [TestMethod]
        public void GetWordCount_Boolean_Returns1()
        {
            var result = InvokeGetWordCount(TypeCode.Boolean);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetWordCount_Int16_Returns1()
        {
            var result = InvokeGetWordCount(TypeCode.Int16);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetWordCount_Int32_Returns2()
        {
            var result = InvokeGetWordCount(TypeCode.Int32);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void GetWordCount_Single_Returns2()
        {
            var result = InvokeGetWordCount(TypeCode.Single);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void GetWordCount_Double_Returns4()
        {
            var result = InvokeGetWordCount(TypeCode.Double);
            Assert.AreEqual(4, result);
        }

        #endregion

        #region BuildReadPayload 读取报文构建测试

        [TestMethod]
        public void BuildReadPayload_BuildsCorrectStructure()
        {
            byte[] payload = InvokeBuildReadPayload(0x82, 100, 5);
            Assert.AreEqual(5, payload.Length);
            Assert.AreEqual((byte)0x82, payload[0]);
            Assert.AreEqual((byte)(100 >> 8), payload[1]);
            Assert.AreEqual((byte)(100 & 0xFF), payload[2]);
            Assert.AreEqual((byte)(5 >> 8), payload[3]);
            Assert.AreEqual((byte)(5 & 0xFF), payload[4]);
        }

        [TestMethod]
        public void BuildReadPayload_LargeAddress_HandlesHighByte()
        {
            byte[] payload = InvokeBuildReadPayload(0xB0, 0x1234, 10);
            Assert.AreEqual((byte)0x12, payload[1]);
            Assert.AreEqual((byte)0x34, payload[2]);
        }

        #endregion

        #region ParseFinsData FINS 响应数据解析测试

        [TestMethod]
        public void ParseFinsData_Boolean_ReturnsCorrectValues()
        {
            byte[] data = { 0x01, 0x00 };
            var result = InvokeParseFinsData(data, TypeCode.Boolean, 1);
            Assert.AreEqual(true, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_Int16_ReturnsCorrectValue()
        {
            short expected = 0x1234;
            byte[] data = { (byte)(expected >> 8), (byte)(expected & 0xFF) };
            var result = InvokeParseFinsData(data, TypeCode.Int16, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_Int16Negative_ReturnsCorrectValue()
        {
            short expected = -232;
            byte[] data = { (byte)(expected >> 8), (byte)(expected & 0xFF) };
            var result = InvokeParseFinsData(data, TypeCode.Int16, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_UInt16_ReturnsCorrectValue()
        {
            ushort expected = 60000;
            byte[] data = { (byte)(expected >> 8), (byte)(expected & 0xFF) };
            var result = InvokeParseFinsData(data, TypeCode.UInt16, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_Int32_ReturnsCorrectValue()
        {
            int expected = 333333333;
            byte[] data = {
                (byte)(expected >> 24), (byte)(expected >> 16),
                (byte)(expected >> 8), (byte)(expected & 0xFF)
            };
            var result = InvokeParseFinsData(data, TypeCode.Int32, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_Int32Negative_ReturnsCorrectValue()
        {
            int expected = -666722222;
            byte[] data = {
                (byte)(expected >> 24), (byte)(expected >> 16),
                (byte)(expected >> 8), (byte)(expected & 0xFF)
            };
            var result = InvokeParseFinsData(data, TypeCode.Int32, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_UInt32_ReturnsCorrectValue()
        {
            uint expected = 4000000000;
            byte[] data = {
                (byte)(expected >> 24), (byte)(expected >> 16),
                (byte)(expected >> 8), (byte)(expected & 0xFF)
            };
            var result = InvokeParseFinsData(data, TypeCode.UInt32, 1);
            Assert.AreEqual(expected, result[0]);
        }

        [TestMethod]
        public void ParseFinsData_Single_ReturnsCorrectValue()
        {
            float expected = 23.5555f;
            byte[] fbytes = BitConverter.GetBytes(expected);
            byte[] data = { fbytes[1], fbytes[0], fbytes[3], fbytes[2] };
            var result = InvokeParseFinsData(data, TypeCode.Single, 1);
            Assert.AreEqual(expected, (float)result[0], 1e-4);
        }

        [TestMethod]
        public void ParseFinsData_SingleNegative_ReturnsCorrectValue()
        {
            float expected = -23.5555f;
            byte[] fbytes = BitConverter.GetBytes(expected);
            byte[] data = { fbytes[1], fbytes[0], fbytes[3], fbytes[2] };
            var result = InvokeParseFinsData(data, TypeCode.Single, 1);
            Assert.AreEqual(expected, (float)result[0], 1e-4);
        }

        [TestMethod]
        public void ParseFinsData_Double_ReturnsCorrectValue()
        {
            double expected = 3.14159265358979;
            byte[] dbytes = BitConverter.GetBytes(expected);
            byte[] data = {
                dbytes[1], dbytes[0], dbytes[3], dbytes[2],
                dbytes[5], dbytes[4], dbytes[7], dbytes[6]
            };
            var result = InvokeParseFinsData(data, TypeCode.Double, 1);
            Assert.AreEqual(expected, (double)result[0], 1e-10);
        }

        [TestMethod]
        public void ParseFinsData_DoubleNegative_ReturnsCorrectValue()
        {
            double expected = -23.5555;
            byte[] dbytes = BitConverter.GetBytes(expected);
            byte[] data = {
                dbytes[1], dbytes[0], dbytes[3], dbytes[2],
                dbytes[5], dbytes[4], dbytes[7], dbytes[6]
            };
            var result = InvokeParseFinsData(data, TypeCode.Double, 1);
            Assert.AreEqual(expected, (double)result[0], 1e-10);
        }

        #endregion

        #region ValuesToFinsBytes 值转 FINS 字节测试

        [TestMethod]
        public void ValuesToFinsBytes_Int16_ReturnsCorrectBytes()
        {
            var values = new List<object> { (short)300, (short)(-232) };
            var result = InvokeValuesToFinsBytes(values, TypeCode.Int16);

            short v0 = 300;
            short v1 = -232;
            byte[] expected = {
                (byte)(v0 >> 8), (byte)(v0 & 0xFF),
                (byte)(v1 >> 8), (byte)(v1 & 0xFF)
            };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToFinsBytes_Int32_ReturnsCorrectBytes()
        {
            var values = new List<object> { 333333333, -666722222 };
            var result = InvokeValuesToFinsBytes(values, TypeCode.Int32);

            int v0 = 333333333;
            int v1 = -666722222;
            byte[] expected = {
                (byte)(v0 >> 24), (byte)(v0 >> 16), (byte)(v0 >> 8), (byte)(v0),
                (byte)(v1 >> 24), (byte)(v1 >> 16), (byte)(v1 >> 8), (byte)(v1)
            };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToFinsBytes_UInt32_ReturnsCorrectBytes()
        {
            var values = new List<object> { 4000000000u };
            var result = InvokeValuesToFinsBytes(values, TypeCode.UInt32);

            uint v = 4000000000;
            byte[] expected = {
                (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)(v)
            };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToFinsBytes_Single_ReturnsCorrectBytes()
        {
            float val = 23.5555f;
            var values = new List<object> { val };
            var result = InvokeValuesToFinsBytes(values, TypeCode.Single);

            byte[] fbytes = BitConverter.GetBytes(val);
            byte[] expected = { fbytes[1], fbytes[0], fbytes[3], fbytes[2] };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToFinsBytes_Double_ReturnsCorrectBytes()
        {
            double val = -23.5555;
            var values = new List<object> { val };
            var result = InvokeValuesToFinsBytes(values, TypeCode.Double);

            byte[] dbytes = BitConverter.GetBytes(val);
            byte[] expected = {
                dbytes[1], dbytes[0], dbytes[3], dbytes[2],
                dbytes[5], dbytes[4], dbytes[7], dbytes[6]
            };
            CollectionAssert.AreEqual(expected, result);
        }

        #endregion

        #region BuildWritePayload 写入报文构建测试

        [TestMethod]
        public void BuildWritePayload_Int16_BuildsCorrectStructure()
        {
            var values = new List<object> { (short)300 };
            byte[] payload = InvokeBuildWritePayload(0x82, 100, values, TypeCode.Int16);

            Assert.AreEqual(7, payload.Length);
            Assert.AreEqual((byte)0x82, payload[0]);
            Assert.AreEqual((byte)(100 >> 8), payload[1]);
            Assert.AreEqual((byte)(100 & 0xFF), payload[2]);
            Assert.AreEqual(0, payload[3]);
            Assert.AreEqual(1, payload[4]);

            short v = 300;
            Assert.AreEqual((byte)(v >> 8), payload[5]);
            Assert.AreEqual((byte)(v & 0xFF), payload[6]);
        }

        #endregion

        #region 反射辅助方法

        private static byte InvokeGetAreaCode(string prefix)
        {
            var method = _omronType.GetMethod("GetAreaCode", BindingFlags.NonPublic | BindingFlags.Static);
            return (byte)method.Invoke(null, new object[] { prefix });
        }

        private static int InvokeGetWordCount(TypeCode type)
        {
            var method = _omronType.GetMethod("GetWordCount", BindingFlags.NonPublic | BindingFlags.Static);
            return (int)method.Invoke(null, new object[] { type });
        }

        private static byte[] InvokeBuildReadPayload(byte area, int startWord, int wordCount)
        {
            var method = _omronType.GetMethod("BuildReadPayload", BindingFlags.NonPublic | BindingFlags.Static);
            return (byte[])method.Invoke(null, new object[] { area, startWord, wordCount });
        }

        private static byte[] InvokeBuildWritePayload(byte area, int startWord, List<object> values, TypeCode type)
        {
            var method = _omronType.GetMethod("BuildWritePayload", BindingFlags.NonPublic | BindingFlags.Static);
            return (byte[])method.Invoke(null, new object[] { area, startWord, values, type });
        }

        private static List<object> InvokeParseFinsData(byte[] data, TypeCode type, int count)
        {
            var methods = _omronType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == "ParseFinsData")
                .OrderBy(m => m.GetParameters().Length)
                .ToList();
            var method = methods.First(m => m.GetParameters().Length == 3);
            return (List<object>)method.Invoke(null, new object[] { data, type, count });
        }

        private static byte[] InvokeValuesToFinsBytes(List<object> values, TypeCode type)
        {
            var method = _omronType.GetMethod("ValuesToFinsBytes", BindingFlags.NonPublic | BindingFlags.Static);
            return (byte[])method.Invoke(null, new object[] { values, type });
        }

        #endregion
    }
}
