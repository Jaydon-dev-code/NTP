using Microsoft.VisualStudio.TestTools.UnitTesting;
using McpXLib.Enums;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Domain.Mc1E;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitTestProject.PLC
{
    /// <summary>
    /// MC QNA 1E 协议通信单元测试
    /// <para>覆盖 Mc1ECommunication / Mc1EWirte / Mc1ERead 的帧构建、地址编码、值转换等核心逻辑。</para>
    /// </summary>
    [TestClass]
    public class Mc1ECommunicationTest
    {
        private static readonly Type _writeType = typeof(Mc1EWirte);

        #region EncodeAddress 地址编码测试

        //[TestMethod]
        //public void EncodeAddress_D100_EncodesCorrectly()
        //{
        //    // D 寄存器, 地址 100 (0x64) → [0x64, 0x00, 0x00, 设备码]
        //    byte[] frame = new Mc1ERead().ToByte(Prefix.D, 100, 1);
        //    Assert.AreEqual(0x64, frame[3]);
        //    Assert.AreEqual(0x00, frame[4]);
        //    Assert.AreEqual(0x00, frame[5]);
        //    Assert.AreEqual((byte)Prefix.D, frame[6]);
        //}

        //[TestMethod]
        //public void EncodeAddress_M0_EncodesCorrectly()
        //{
        //    // M 寄存器, 地址 0 → [0x00, 0x00, 0x00, 设备码]
        //    byte[] frame = new Mc1ERead().ToByte(Prefix.M, 0, 1);
        //    Assert.AreEqual(0x00, frame[3]);
        //    Assert.AreEqual(0x00, frame[4]);
        //    Assert.AreEqual(0x00, frame[5]);
        //    Assert.AreEqual((byte)Prefix.M, frame[6]);
        //}

        //[TestMethod]
        //public void EncodeAddress_LargeAddress_HandlesHighBytes()
        //{
        //    // 大地址 0x123456
        //    byte[] frame = new Mc1ERead().ToByte(Prefix.D, 0x123456, 1);
        //    Assert.AreEqual(0x56, frame[3]);
        //    Assert.AreEqual(0x34, frame[4]);
        //    Assert.AreEqual(0x12, frame[5]);
        //    Assert.AreEqual((byte)Prefix.D, frame[6]);
        //}

        //[TestMethod]
        //public void EncodeAddress_X_HexDevice_UsesPrefix()
        //{
        //    byte[] frame = new Mc1ERead().ToByte(Prefix.X, 0, 1);
        //    Assert.AreEqual((byte)Prefix.X, frame[6]);
        //}

       //[TestMethod]
       // public void EncodeAddress_Y_HexDevice_UsesPrefix()
       // { 
       //     byte[] frame = new Mc1ERead().ToByte(Prefix.Y, 100, 1);
       //     Assert.AreEqual((byte)Prefix.Y, frame[6]);
       // }

       // #endregion

       // #region BuildReadFrame 读帧构建测试

       // [TestMethod]
       // public void BuildReadFrame_ValidParams_ReturnsFrame()
       // {
       //     byte[] frame = new Mc1ERead().ToByte(Prefix.D, 100, 5);
       //     // 帧结构：[功能码][PLC编号][超时2B][地址4B][设备码2B][字数2B] = 12B
       //     Assert.AreEqual(12, frame.Length);
       //     Assert.AreEqual(0x01, frame[0]); // 功能码
       //     Assert.AreEqual(0xFF, frame[1]); // PLC编号
       //     Assert.AreEqual(0x00, frame[2]); // 超时低
       //     Assert.AreEqual(0x00, frame[3]); // 超时高
       //     Assert.AreEqual(0x64, frame[4]); // 地址(100)=0x64
       //     Assert.AreEqual(0x00, frame[5]);
       //     Assert.AreEqual(0x00, frame[6]);
       //     Assert.AreEqual((byte)Prefix.D, frame[7]); // 设备码
       //     Assert.AreEqual((byte)Prefix.D, frame[8]); // McPrefix
       //     Assert.AreEqual(0x00, frame[9]);
       //     Assert.AreEqual(0x05, frame[10]); // 字数 5
       //     Assert.AreEqual(0x00, frame[11]);
        
       //     Assert.AreEqual(12, frame.Length);
       //     Assert.AreEqual(0x01, frame[0]); // 功能码
       //     Assert.AreEqual(0xFF, frame[1]); // PLC编号
       //     Assert.AreEqual(0x00, frame[2]); // 超时低
       //     Assert.AreEqual(0x00, frame[3]); // 超时高
       //     Assert.AreEqual(0x64, frame[4]); // 地址(100)=0x64
       //     Assert.AreEqual(0x00, frame[5]);
       //     Assert.AreEqual(0x00, frame[6]);
       //     Assert.AreEqual((byte)Prefix.D, frame[7]); // 设备码
       //     Assert.AreEqual((byte)Prefix.D, frame[8]); // McPrefix
       //     Assert.AreEqual(0x00, frame[9]);
       //     Assert.AreEqual(0x05, frame[10]); // 字数 5
       //     Assert.AreEqual(0x00, frame[11]);
       // }

       // [TestMethod]
       // public void BuildReadFrame_ZeroWords_SendsZero()
       // {
       //     byte[] frame = new Mc1ERead().ToByte(Prefix.M, 0, 0);
       //     Assert.AreEqual(0x00, frame[10]);
       //     Assert.AreEqual(0x00, frame[11]);
       // }

       // [TestMethod]
       // public void BuildReadFrame_MaxWords_UsesUshort()
       // {
       //     byte[] frame = new Mc1ERead().ToByte(Prefix.D, 0, 65535);
       //     Assert.AreEqual(0xFF, frame[10]);
       //     Assert.AreEqual(0xFF, frame[11]);
       // }

       // [TestMethod]
       // public void BuildReadFrame_DifferentPrefix_EncodesCorrectly()
       // {
       //     byte[] frame = new Mc1ERead().ToByte(Prefix.SM, 50, 10);
       //     Assert.AreEqual((byte)Prefix.SM, frame[7]);
       //     Assert.AreEqual((byte)Prefix.SM, frame[8]);
       // }

        #endregion

        #region GetResponseData 响应解析测试

        [TestMethod]
        public void GetResponseData_Success_ReturnsData()
        {
            byte[] resp = { 0x00, 0x12, 0x34 };
            byte[] data = Mc1ERead.GetResponseData(resp);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34 }, data);
        }

        [TestMethod]
        public void GetResponseData_NoData_ReturnsEmpty()
        {
            byte[] resp = { 0x00 };
            byte[] data = Mc1ERead.GetResponseData(resp);
            Assert.AreEqual(0, data.Length);
        }

        [TestMethod]
        public void GetResponseData_ErrorCode_Throws()
        {
            try
            {
                Mc1ERead.GetResponseData(new byte[] { 0x05 });
                Assert.Fail("应抛出异常");
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Mc1EWirte ToByte 写入帧构建测试

        [TestMethod]
        public void ToByte_Int16_SingleValue_ProducesValidFrame()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", (short)1234);
            // 帧：[功能码1][PLC编号1][超时2][地址4][设备码2][字数2][数据2] = 14B
            Assert.IsNotNull(frame);
            Assert.IsTrue(frame.Length >= 12);
            // 功能码：字写入 0x03
            Assert.AreEqual(0x03, frame[0]);
            // PLC编号 0xFF
            Assert.AreEqual(0xFF, frame[1]);
            // 设备码
            Assert.AreEqual((byte)Prefix.D, frame[7]);
            // 字数 1
            Assert.AreEqual(0x01, frame[10]);
            Assert.AreEqual(0x00, frame[11]);
            // 数据 2 字节（小端 1234 = 0x04D2）
            Assert.AreEqual(0xD2, frame[12]);
            Assert.AreEqual(0x04, frame[13]);
        }

        [TestMethod]
        public void ToByte_Int16_MultipleValues_ProducesCorrectWordCount()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", new short[] { 1, 2, 3 });
            // 字数 = 3
            Assert.AreEqual(0x03, frame[10]);
            Assert.AreEqual(0x00, frame[11]);
            // 数据 6 字节
            Assert.AreEqual(14 + 6 - 1, frame.Length - 1); // 14 + 6 - 1 for 0-index
        }

        [TestMethod]
        public void ToByte_Int32_SingleValue_WordCountIs2()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", 33333333);
            // Int32 占 2 字
            Assert.AreEqual(0x02, frame[10]);
        }

        [TestMethod]
        public void ToByte_Single_WordCountIs2()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", 3.14f);
            Assert.AreEqual(0x02, frame[10]);
        }

        [TestMethod]
        public void ToByte_Double_WordCountIs4()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", 3.14159265358979);
            Assert.AreEqual(0x04, frame[10]);
        }

        [TestMethod]
        public void ToByte_Boolean_WordCountIsCount()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.M, "0", new bool[] { true, false, true });
            // 功能码应为位写入 0x02
            Assert.AreEqual(0x02, frame[0]);
            // 字数 = 3 bits
            Assert.AreEqual(0x03, frame[10]);
        }

        [TestMethod]
        public void ToByte_Boolean_True_DataIs0100()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.M, "0", true);
            // true → short(1) → [0x01, 0x00]
            Assert.AreEqual(0x01, frame[12]);
            Assert.AreEqual(0x00, frame[13]);
        }

        [TestMethod]
        public void ToByte_Boolean_False_DataIs0000()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.M, "0", false);
            Assert.AreEqual(0x00, frame[12]);
            Assert.AreEqual(0x00, frame[13]);
        }

        [TestMethod]
        public void ToByte_String_Ascii_EncodesCorrectly()
        {
            // "ABC" → Shift-JIS 编码为 [0x41, 0x42, 0x43] → 每字节占 1 字
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", "ABC");
            // 字数 = 3（3 个字符）
            Assert.AreEqual(0x03, frame[10]);
            Assert.AreEqual(0x00, frame[11]);
            // 数据：每个字节后补 0x00
            Assert.AreEqual(0x41, frame[12]); // 'A'
            Assert.AreEqual(0x00, frame[13]);
            Assert.AreEqual(0x42, frame[14]); // 'B'
            Assert.AreEqual(0x00, frame[15]);
            Assert.AreEqual(0x43, frame[16]); // 'C'
            Assert.AreEqual(0x00, frame[17]);
        }

        [TestMethod]
        public void ToByte_String_Empty_ProducesZeroWords()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", "");
            Assert.AreEqual(0x00, frame[10]);
            Assert.AreEqual(0x00, frame[11]);
        }

        [TestMethod]
        public void ToByte_DecimalDevice_AddressIsDecimal()
        {
            // D 寄存器(十进制设备)，地址 "100" = 0x64
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", (short)0);
            // 地址小端：100 = 0x64
            Assert.AreEqual(0x64, frame[4]);
            Assert.AreEqual(0x00, frame[5]);
            Assert.AreEqual(0x00, frame[6]);
            Assert.AreEqual((byte)Prefix.D, frame[7]);
        }

        [TestMethod]
        public void ToByte_XWithHexAddress_ParsesAsHex()
        {
            // X 寄存器(十六进制设备)，地址 "FF" = 255
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.X, "FF", true);
            // FF = 0x000000FF
            Assert.AreEqual(0xFF, frame[4]);
            Assert.AreEqual(0x00, frame[5]);
            Assert.AreEqual(0x00, frame[6]);
            Assert.AreEqual((byte)Prefix.X, frame[7]);
        }

        [TestMethod]
        public void ToByte_FunctionCode_BitWrite_Is02()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.M, "0", true);
            Assert.AreEqual(0x02, frame[0]); // BatchBitWrite
        }

        [TestMethod]
        public void ToByte_FunctionCode_WordWrite_Is03()
        {
            var writer = new Mc1EWirte();
            byte[] frame = writer.ToByte(Prefix.D, "100", (short)0);
            Assert.AreEqual(0x03, frame[0]); // BatchWordWrite
        }

        [TestMethod]
        public void ToByte_NonGeneric_Int16_ProducesSameResult()
        {
            var writer = new Mc1EWirte();
            var values = new List<object> { (short)1234 };
            byte[] frame = writer.ToByte(Prefix.D, "100", TypeCode.Int16, values);
            Assert.AreEqual(0x03, frame[0]);
            Assert.AreEqual(0x01, frame[10]); // 字数 1
            Assert.AreEqual(0xD2, frame[12]);
            Assert.AreEqual(0x04, frame[13]);
        }

        [TestMethod]
        public void ToByte_NonGeneric_String_EncodesCorrectly()
        {
            var writer = new Mc1EWirte();
            var values = new List<object> { "Hello" };
            byte[] frame = writer.ToByte(Prefix.D, "100", TypeCode.String, values);
            Assert.AreEqual(0x03, frame[0]);
            Assert.AreEqual(0x05, frame[10]); // 字数 5
            Assert.AreEqual(0x48, frame[12]); // 'H'
            Assert.AreEqual(0x00, frame[13]);
        }

        [TestMethod]
        public void ToByte_NonGeneric_Boolean_IsBitWrite()
        {
            var writer = new Mc1EWirte();
            var values = new List<object> { true, false };
            byte[] frame = writer.ToByte(Prefix.M, "0", TypeCode.Boolean, values);
            Assert.AreEqual(0x02, frame[0]); // BatchBitWrite
            Assert.AreEqual(0x02, frame[10]); // 位数 2
        }

        #endregion

        #region Mc1EWirte IsHexDevice 测试

        [TestMethod]
        public void IsHexDevice_X_ReturnsTrue()
        {
            Assert.IsTrue(InvokeIsHexDevice(Prefix.X));
        }

        [TestMethod]
        public void IsHexDevice_Y_ReturnsTrue()
        {
            Assert.IsTrue(InvokeIsHexDevice(Prefix.Y));
        }

        [TestMethod]
        public void IsHexDevice_B_ReturnsTrue()
        {
            Assert.IsTrue(InvokeIsHexDevice(Prefix.B));
        }

        [TestMethod]
        public void IsHexDevice_W_ReturnsTrue()
        {
            Assert.IsTrue(InvokeIsHexDevice(Prefix.W));
        }

        [TestMethod]
        public void IsHexDevice_D_ReturnsFalse()
        {
            Assert.IsFalse(InvokeIsHexDevice(Prefix.D));
        }

        [TestMethod]
        public void IsHexDevice_M_ReturnsFalse()
        {
            Assert.IsFalse(InvokeIsHexDevice(Prefix.M));
        }

        [TestMethod]
        public void IsHexDevice_SM_ReturnsFalse()
        {
            Assert.IsFalse(InvokeIsHexDevice(Prefix.SM));
        }

        [TestMethod]
        public void IsHexDevice_ZR_ReturnsFalse()
        {
            Assert.IsFalse(InvokeIsHexDevice(Prefix.ZR));
        }

        [TestMethod]
        public void IsHexDevice_DY_ReturnsTrue()
        {
            Assert.IsTrue(InvokeIsHexDevice(Prefix.DY));
        }

        #endregion

        #region Mc1EWirte 地址校验测试

        [TestMethod]
        public void ValidateAddress_D_Decimal_ReturnsTrue()
        {
            Assert.IsTrue(InvokeValidateAddress(Prefix.D, "100"));
        }

        [TestMethod]
        public void ValidateAddress_D_NonNumeric_ReturnsFalse()
        {
            Assert.IsFalse(InvokeValidateAddress(Prefix.D, "ABC"));
        }

        [TestMethod]
        public void ValidateAddress_X_Hex_ReturnsTrue()
        {
            Assert.IsTrue(InvokeValidateAddress(Prefix.X, "1A2B"));
        }

        [TestMethod]
        public void ValidateAddress_X_InvalidHex_ReturnsFalse()
        {
            Assert.IsFalse(InvokeValidateAddress(Prefix.X, "XYZ"));
        }

        [TestMethod]
        public void ValidateAddress_M_Decimal_ReturnsTrue()
        {
            Assert.IsTrue(InvokeValidateAddress(Prefix.M, "255"));
        }

        #endregion

        #region Mc1EWirte GetTypeOfShortOffset 测试

        [TestMethod]
        public void GetTypeOfShortOffset_Boolean_Returns1()
        {
            Assert.AreEqual(1, InvokeGetTypeOfShortOffset(TypeCode.Boolean));
        }

        [TestMethod]
        public void GetTypeOfShortOffset_Int16_Returns1()
        {
            Assert.AreEqual(1, InvokeGetTypeOfShortOffset(TypeCode.Int16));
        }

        [TestMethod]
        public void GetTypeOfShortOffset_Int32_Returns2()
        {
            Assert.AreEqual(2, InvokeGetTypeOfShortOffset(TypeCode.Int32));
        }

        [TestMethod]
        public void GetTypeOfShortOffset_Single_Returns2()
        {
            Assert.AreEqual(2, InvokeGetTypeOfShortOffset(TypeCode.Single));
        }

        [TestMethod]
        public void GetTypeOfShortOffset_Double_Returns4()
        {
            Assert.AreEqual(4, InvokeGetTypeOfShortOffset(TypeCode.Double));
        }

        [TestMethod]
        public void GetTypeOfShortOffset_String_Returns1()
        {
            Assert.AreEqual(1, InvokeGetTypeOfShortOffset(TypeCode.String));
        }

        #endregion

        #region Mc1EWirte 值转换测试

        [TestMethod]
        public void ValuesToBytes_Int16_ReturnsLittleEndian()
        {
            byte[] result = InvokeValuesToBytes(new short[] { 300, -232 }, TypeCode.Int16);
            byte[] expected = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes((short)300), 0, expected, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((short)(-232)), 0, expected, 2, 2);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToBytes_Int32_ReturnsLittleEndian()
        {
            byte[] result = InvokeValuesToBytes(new int[] { 33333333, -66672222 }, TypeCode.Int32);
            byte[] expected = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(33333333), 0, expected, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(-66672222), 0, expected, 4, 4);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValuesToBytes_UInt32_ReturnsLittleEndian()
        {
            byte[] result = InvokeValuesToBytes(new uint[] { 4000000000 }, TypeCode.UInt32);
            CollectionAssert.AreEqual(BitConverter.GetBytes(4000000000u), result);
        }

        [TestMethod]
        public void ValuesToBytes_Single_ReturnsLittleEndian()
        {
            byte[] result = InvokeValuesToBytes(new float[] { 23.5555f }, TypeCode.Single);
            CollectionAssert.AreEqual(BitConverter.GetBytes(23.5555f), result);
        }

        [TestMethod]
        public void ValuesToBytes_Double_ReturnsLittleEndian()
        {
            byte[] result = InvokeValuesToBytes(new double[] { -23.5555 }, TypeCode.Double);
            CollectionAssert.AreEqual(BitConverter.GetBytes(-23.5555), result);
        }

        [TestMethod]
        public void ValuesToBytes_Boolean_ReturnsShort0or1()
        {
            byte[] result = InvokeValuesToBytes(new bool[] { true, false }, TypeCode.Boolean);
            byte[] expected = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, expected, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((short)0), 0, expected, 2, 2);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToBytes_Int16_ReturnsTwoBytes()
        {
            var method = _writeType.GetMethod("ValueToBytes", BindingFlags.NonPublic | BindingFlags.Static);
            byte[] result = (byte[])method.Invoke(null, new object[] { (short)-232, TypeCode.Int16 });
            CollectionAssert.AreEqual(BitConverter.GetBytes((short)-232), result);
        }

        [TestMethod]
        public void ValueToBytes_Int32_ReturnsFourBytes()
        {
            var method = _writeType.GetMethod("ValueToBytes", BindingFlags.NonPublic | BindingFlags.Static);
            byte[] result = (byte[])method.Invoke(null, new object[] { 33333333, TypeCode.Int32 });
            CollectionAssert.AreEqual(BitConverter.GetBytes(33333333), result);
        }

        #endregion

        #region 公共接口基本测试

        [TestMethod]
        public void Read_UnreachablePlc_ReturnsFail()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateReadDto(100, TypeCode.Int16, 1);
            var result = comm.Read(dto);
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Read_UnreachablePlc_ReturnsNonNullResult()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateReadDto(100, TypeCode.Int16, 1);
            var result = comm.Read(dto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Read_UnreachablePlc_FillsZeroValue()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateReadDto(100, TypeCode.Int16, 1);
            var result = comm.Read(dto);
            Assert.IsNotNull(dto.Value);
            Assert.AreEqual(1, dto.Value.Count);
            Assert.AreEqual(0, dto.Value[0]);
        }

        [TestMethod]
        public void Write_UnreachablePlc_ReturnsFail()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateWriteDto(100, TypeCode.Int16, new List<object> { (short)123 });
            var result = comm.Write(dto);
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Write_BatchList_ThrowsNotImplemented()
        {
            var comm = new Mc1ECommunication();
            try
            {
                comm.Write(new List<DevPlcPointDto>());
                Assert.Fail("应抛出 NotImplementedException");
            }
            catch (NotImplementedException)
            {
            }
        }

        [TestMethod]
        public void Write_InvalidPrefix_ReturnsFail()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateWriteDto(100, TypeCode.Int16, new List<object> { (short)123 }, prefix: "INVALID");
            var result = comm.Write(dto);
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Write_VariousTypes_ReturnsResult()
        {
            var comm = new Mc1ECommunication();

            //Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.Int16, new List<object> { (short)1 })));
            //Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.Int32, new List<object> { 123 })));
            //Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.Single, new List<object> { 1.5f })));
            //Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.Double, new List<object> { 3.14 })));
            //Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.Boolean, new List<object> { true })));
            Assert.IsNotNull(comm.Write(CreateWriteDto(100, TypeCode.String, new List<object> { "hello word !!" })));
        }

        [TestMethod]
        public void Read_UnsupportedType_ReturnsResult()
        {
            var comm = new Mc1ECommunication();
            var dto = CreateReadDto(100, TypeCode.Decimal, 1);
            var result = comm.Read(dto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Read_Batch_UnreachablePlc_ReturnsFail()
        {
            var comm = new Mc1ECommunication();
            var list = new List<DevPlcPointDto>
            {
                CreateReadDto(100, TypeCode.Int16, 1),
                CreateReadDto(200, TypeCode.Int32, 2),
            };
            var result = comm.Read(list);
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Read_Batch_X()
        {
            var comm = new Mc1ECommunication();
            var list = new List<DevPlcPointDto>
            {
                CreateReadDto(10, TypeCode.Boolean, 1,"X"),
                CreateReadDto(11, TypeCode.Boolean, 1,"X"),
            };
            var result = comm.Read(list);
            Assert.IsFalse(result.IsSuccess);
        }
        [TestMethod]
        public void Read_Batch_String()
        {
            var comm = new Mc1ECommunication();
            var list = new List<DevPlcPointDto>
            {
                CreateReadDto(100, TypeCode.String, 6),
            
            };
            var result = comm.Read(list);
            Assert.IsFalse(result.IsSuccess);
        }

        #endregion

        #region 辅助方法

        private static DevPlcPointDto CreateReadDto(int address, TypeCode dataType, int length, string prefix = "D")
        {
            return new DevPlcPointDto
            {
                IpAddress = "127.0.0.1",
                Port = 6000,
                Prefix = prefix,
                DataType = dataType,
                Address = address.ToString(),
                Length = length,
            };
        }

        private static DevPlcPointDto CreateWriteDto(int address, TypeCode dataType, List<object> value, string prefix = "D")
        {
            return new DevPlcPointDto
            {
                IpAddress = "127.0.0.1",
                Port = 6000,
                Prefix = prefix,
                DataType = dataType,
                Address = address.ToString(),
                Length = value?.Count ?? 1,
                Value = value,
            };
        }

        #endregion

        #region 反射辅助方法

        private static bool InvokeIsHexDevice(Prefix prefix)
        {
            var method = _writeType.GetMethod("IsHexDevice", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)method.Invoke(new Mc1EWirte(), new object[] { prefix });
        }

        private static bool InvokeValidateAddress(Prefix prefix, string address)
        {
            var method = _writeType.GetMethod("ValidateAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)method.Invoke(new Mc1EWirte(), new object[] { prefix, address });
        }

        private static int InvokeGetTypeOfShortOffset(TypeCode type)
        {
            var method = _writeType.GetMethod("GetTypeOfShortOffset", BindingFlags.NonPublic | BindingFlags.Static);
            return (int)method.Invoke(null, new object[] { type });
        }

        private static byte[] InvokeValuesToBytes<T>(T[] values, TypeCode typeCode)
        {
            var method = _writeType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "ValuesToBytes" && m.IsGenericMethod);
            return (byte[])method.Invoke(null, new object[] { values, typeCode });
        }

        #endregion
    }
}
