using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitTestProject.PLC
{
    [TestClass]
    public class S7CommunicationTest
    {
        private static readonly Type _s7Type = typeof(S7Communication);

        [TestMethod]
        public void cc()
        {
            S7Communication s7Communication=new S7Communication();
           var a= s7Communication.Read(new SL.MLineDataPrecisionTracking.Models.Dtos.DevPlcPointDto("","","127.0.0.1",102, "DB1", TypeCode.String,"DBD11",3) );
        }

        #region ParseAddress 地址解析测试

        [TestMethod]
        public void ParseAddress_DotNotation_ReturnsCorrectOffsets()
        {
            var result = InvokeParseAddress("100.3");
            Assert.AreEqual(100, result.byteOffset);
            Assert.AreEqual(3, result.bitOffset);
        }

        [TestMethod]
        public void ParseAddress_WithoutDot_ReturnsZeroBitOffset()
        {
            var result = InvokeParseAddress("200");
            Assert.AreEqual(200, result.byteOffset);
            Assert.AreEqual(0, result.bitOffset);
        }

        [TestMethod]
        public void ParseAddress_ZeroAddress_ReturnsZeroOffsets()
        {
            var result = InvokeParseAddress("0.0");
            Assert.AreEqual(0, result.byteOffset);
            Assert.AreEqual(0, result.bitOffset);
        }

        [TestMethod]
        public void ParseAddress_MaxBitOffset_ReturnsSeven()
        {
            var result = InvokeParseAddress("50.7");
            Assert.AreEqual(50, result.byteOffset);
            Assert.AreEqual(7, result.bitOffset);
        }

        [TestMethod]
        public void ParseAddress_OnlyBitOffset_DefaultsByteOffsetToZero()
        {
            var result = InvokeParseAddress(".5");
            Assert.AreEqual(0, result.byteOffset);
            Assert.AreEqual(5, result.bitOffset);
        }

        #endregion

        #region ConvertBytes 布尔和单字节读取测试

        [TestMethod]
        public void ConvertBytes_BooleanTrue_ReturnsTrue()
        {
            byte[] buffer = { 0x01 };
            var result = InvokeConvertBytes(buffer, TypeCode.Boolean, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_BooleanFalse_ReturnsFalse()
        {
            byte[] buffer = { 0x00 };
            var result = InvokeConvertBytes(buffer, TypeCode.Boolean, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(false, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_Byte_ReturnsCorrectValue()
        {
            byte[] buffer = { 0xAB };
            var result = InvokeConvertBytes(buffer, TypeCode.Byte, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual((byte)0xAB, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_ByteMinValue_ReturnsZero()
        {
            byte[] buffer = { 0x00 };
            var result = InvokeConvertBytes(buffer, TypeCode.Byte, 1);
            Assert.AreEqual((byte)0, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_ByteMaxValue_Returns255()
        {
            byte[] buffer = { 0xFF };
            var result = InvokeConvertBytes(buffer, TypeCode.Byte, 1);
            Assert.AreEqual((byte)255, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_ByteMultiple_ReturnsAllValues()
        {
            byte[] buffer = { 0x01, 0x02, 0x03 };
            var result = InvokeConvertBytes(buffer, TypeCode.Byte, 3);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual((byte)1, result[0]);
            Assert.AreEqual((byte)2, result[1]);
            Assert.AreEqual((byte)3, result[2]);
        }

        #endregion

        #region ConvertBytes 浮点和双精度读取测试

        [TestMethod]
        public void ConvertBytes_Double_ReturnsCorrectValue()
        {
            double expected = 3.14159265358979;
            byte[] buffer = BitConverter.GetBytes(expected);
            var result = InvokeConvertBytes(buffer, TypeCode.Double, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, (double)result[0], 1e-10);
        }

        [TestMethod]
        public void ConvertBytes_DoubleNegative_ReturnsCorrectValue()
        {
            double expected = -23.5555;
            byte[] buffer = BitConverter.GetBytes(expected);
            var result = InvokeConvertBytes(buffer, TypeCode.Double, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, (double)result[0], 1e-10);
        }

        [TestMethod]
        public void ConvertBytes_DoubleMultiple_ReturnsAllValues()
        {
            double[] expected = { 1.5, -2.5, 3.0 };
            byte[] buffer = new byte[24];
            Buffer.BlockCopy(BitConverter.GetBytes(expected[0]), 0, buffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(expected[1]), 0, buffer, 8, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(expected[2]), 0, buffer, 16, 8);
            var result = InvokeConvertBytes(buffer, TypeCode.Double, 3);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(expected[0], (double)result[0], 1e-10);
            Assert.AreEqual(expected[1], (double)result[1], 1e-10);
            Assert.AreEqual(expected[2], (double)result[2], 1e-10);
        }

        [TestMethod]
        public void ConvertBytes_Single_ReturnsCorrectValue()
        {
            float expected = 23.5555f;
            byte[] buffer = BitConverter.GetBytes(expected);
            var result = InvokeConvertBytes(buffer, TypeCode.Single, 1);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, (float)result[0], 1e-6f);
        }

        [TestMethod]
        public void ConvertBytes_SingleMultiple_ReturnsAllValues()
        {
            float[] expected = { 1.5f, -2.5f, 3.0f };
            byte[] buffer = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(expected[0]), 0, buffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(expected[1]), 0, buffer, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(expected[2]), 0, buffer, 8, 4);
            var result = InvokeConvertBytes(buffer, TypeCode.Single, 3);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(expected[0], (float)result[0], 1e-6f);
            Assert.AreEqual(expected[1], (float)result[1], 1e-6f);
            Assert.AreEqual(expected[2], (float)result[2], 1e-6f);
        }

        #endregion

        #region ConvertBytes 带偏移量读取测试

        [TestMethod]
        public void ConvertBytes_WithOffset_SkipsLeadingBytes()
        {
            byte[] buffer = { 0xFF, 0xFF, 0x42 };
            var result = InvokeConvertBytes(buffer, 2, TypeCode.Byte, 1);
            Assert.AreEqual((byte)0x42, result[0]);
        }

        [TestMethod]
        public void ConvertBytes_WithOffset_MultipleValues_ReadsFromCorrectPosition()
        {
            byte[] buffer = { 0xFF, 0x01, 0x02, 0x03 };
            var result = InvokeConvertBytes(buffer, 1, TypeCode.Byte, 3);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual((byte)1, result[0]);
            Assert.AreEqual((byte)2, result[1]);
            Assert.AreEqual((byte)3, result[2]);
        }

        [TestMethod]
        public void ConvertBytes_WithOffset_ZeroOffset_ReturnsFirstByte()
        {
            byte[] buffer = { 0x42, 0xFF };
            var result = InvokeConvertBytes(buffer, 0, TypeCode.Byte, 1);
            Assert.AreEqual((byte)0x42, result[0]);
        }

        #endregion

        #region ToS7Bytes 值转字节测试

        [TestMethod]
        public void ToS7Bytes_Byte_ReturnsSingleByte()
        {
            var result = InvokeToS7Bytes((byte)42, TypeCode.Byte);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(42, result[0]);
        }

        [TestMethod]
        public void ToS7Bytes_ByteMinValue_ReturnsZero()
        {
            var result = InvokeToS7Bytes((byte)0, TypeCode.Byte);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(0, result[0]);
        }

        [TestMethod]
        public void ToS7Bytes_ByteMaxValue_Returns255()
        {
            var result = InvokeToS7Bytes((byte)255, TypeCode.Byte);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(255, result[0]);
        }

        [TestMethod]
        public void ToS7Bytes_Int16_ReturnsLittleEndianBytes()
        {
            short value = -232;
            var result = InvokeToS7Bytes(value, TypeCode.Int16);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_Int16Zero_ReturnsTwoZeroBytes()
        {
            short value = 0;
            var result = InvokeToS7Bytes(value, TypeCode.Int16);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(0, result[1]);
        }

        [TestMethod]
        public void ToS7Bytes_Int16MaxValue_ReturnsCorrectBytes()
        {
            short value = short.MaxValue;
            var result = InvokeToS7Bytes(value, TypeCode.Int16);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_Int16MinValue_ReturnsCorrectBytes()
        {
            short value = short.MinValue;
            var result = InvokeToS7Bytes(value, TypeCode.Int16);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_UInt16_ReturnsLittleEndianBytes()
        {
            ushort value = 60000;
            var result = InvokeToS7Bytes(value, TypeCode.UInt16);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_Int32_ReturnsLittleEndianBytes()
        {
            int value = 333333333;
            var result = InvokeToS7Bytes(value, TypeCode.Int32);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_UInt32_ReturnsLittleEndianBytes()
        {
            uint value = 4000000000;
            var result = InvokeToS7Bytes(value, TypeCode.UInt32);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_Single_ReturnsLittleEndianBytes()
        {
            float value = 23.5555f;
            var result = InvokeToS7Bytes(value, TypeCode.Single);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        [TestMethod]
        public void ToS7Bytes_Double_ReturnsLittleEndianBytes()
        {
            double value = -23.5555;
            var result = InvokeToS7Bytes(value, TypeCode.Double);
            CollectionAssert.AreEqual(BitConverter.GetBytes(value), result);
        }

        #endregion

        #region 辅助方法

        private static (int byteOffset, int bitOffset) InvokeParseAddress(string address)
        {
            var method = _s7Type.GetMethod("ParseAddress", BindingFlags.NonPublic | BindingFlags.Static);
            var parameters = new object[] { address, 0, 0 };
            method.Invoke(null, parameters);
            return ((int)parameters[1], (int)parameters[2]);
        }

        private static List<object> InvokeConvertBytes(byte[] buffer, TypeCode type, int count)
        {
            var method = _s7Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "ConvertBytes" && m.GetParameters().Length == 3);
            return (List<object>)method.Invoke(null, new object[] { buffer, type, count });
        }

        private static List<object> InvokeConvertBytes(byte[] buffer, int start, TypeCode type, int count)
        {
            var method = _s7Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "ConvertBytes" && m.GetParameters().Length == 4);
            return (List<object>)method.Invoke(null, new object[] { buffer, start, type, count });
        }

        private static byte[] InvokeToS7Bytes(object value, TypeCode type)
        {
            var method = _s7Type.GetMethod("ToS7Bytes", BindingFlags.NonPublic | BindingFlags.Static);
            return (byte[])method.Invoke(null, new[] { value, type });
        }

        #endregion
    }
}
