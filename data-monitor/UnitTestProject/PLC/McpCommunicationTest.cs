using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitTestProject.PLC
{
    /// <summary>
    /// MCP 通信单元测试（测试 GroupByAddress 地址分组逻辑）
    /// 分组规则：相邻地址差 ≤ 128 归为一组，> 128 断开新组
    /// </summary>
    [TestClass]
    public class McpCommunicationTest
    {
        #region 边界情况测试

        [TestMethod]
        public void GroupByAddress_EmptyList_ReturnsEmpty()
        {
            var result = InvokeGroupByAddress(new List<DevPlcPointReadDto>());
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GroupByAddress_SingleElement_ReturnsOneGroup()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Count);
        }

        #endregion

        #region 连续地址分组测试

        [TestMethod]
        public void GroupByAddress_ContinuousAddresses_GroupsTogether()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100),
                MakeReadDto(address: 101),
                MakeReadDto(address: 102)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Count);
        }

        [TestMethod]
        public void GroupByAddress_GapLessThanEqual128_GroupsTogether()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0),
                MakeReadDto(address: 128)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Count);
        }

        [TestMethod]
        public void GroupByAddress_GapGreaterThan128_SplitsGroup()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0),
                MakeReadDto(address: 129)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Count);
            Assert.AreEqual(1, result[1].Count);
        }

        [TestMethod]
        public void GroupByAddress_MultipleGroups_SplitsCorrectly()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 10),
                MakeReadDto(address: 20),
                MakeReadDto(address: 200),
                MakeReadDto(address: 210)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].Count);
            Assert.AreEqual(10, result[0][0].Address);
            Assert.AreEqual(20, result[0][1].Address);
            Assert.AreEqual(200, result[1][0].Address);
            Assert.AreEqual(210, result[1][1].Address);
        }

        [TestMethod]
        public void GroupByAddress_ThreeGroups_SplitsCorrectly()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0),
                MakeReadDto(address: 1),
                MakeReadDto(address: 140),
                MakeReadDto(address: 141),
                MakeReadDto(address: 300),
                MakeReadDto(address: 301)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0].Count);
            Assert.AreEqual(2, result[1].Count);
            Assert.AreEqual(2, result[2].Count);
        }

        #endregion

        #region 边界值测试（128/129 临界点）

        [TestMethod]
        public void GroupByAddress_GapExactly128_StaysInSameGroup()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0),
                MakeReadDto(address: 128)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GroupByAddress_GapExactly129_SplitsIntoNewGroup()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0),
                MakeReadDto(address: 129)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
        }

        #endregion

        #region 输入顺序测试

        [TestMethod]
        public void GroupByAddress_UnsortedInput_SortsBeforeGrouping()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 200),
                MakeReadDto(address: 10),
                MakeReadDto(address: 300),
                MakeReadDto(address: 20)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(10, result[0][0].Address);
            Assert.AreEqual(20, result[0][1].Address);
            Assert.AreEqual(200, result[1][0].Address);
            Assert.AreEqual(300, result[1][1].Address);
        }

        [TestMethod]
        public void GroupByAddress_SameAddress_ShouldBeInOrder()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100),
                MakeReadDto(address: 100)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Count);
        }

        #endregion

        #region 不同数据类型分组测试

        [TestMethod]
        public void GroupByAddress_MixedTypesContinuous_GroupsTogether()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100, dataType: TypeCode.Int16, shortOffset: 1),
                MakeReadDto(address: 101, dataType: TypeCode.Boolean, shortOffset: 1),
                MakeReadDto(address: 102, dataType: TypeCode.Int32, shortOffset: 2),
                MakeReadDto(address: 104, dataType: TypeCode.Single, shortOffset: 2)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(4, result[0].Count);
        }

        [TestMethod]
        public void GroupByAddress_MixedTypesWithGap_SplitsCorrectly()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0, dataType: TypeCode.Int16, shortOffset: 1),
                MakeReadDto(address: 1, dataType: TypeCode.Boolean, shortOffset: 1),
                MakeReadDto(address: 200, dataType: TypeCode.Int32, shortOffset: 2),
                MakeReadDto(address: 202, dataType: TypeCode.Double, shortOffset: 4)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].Count);
            Assert.AreEqual(2, result[1].Count);
        }

        [TestMethod]
        public void GroupByAddress_MixedTypesLargeGap_CreatesMultipleGroups()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 10, dataType: TypeCode.Int16, shortOffset: 1),
                MakeReadDto(address: 20, dataType: TypeCode.Int32, shortOffset: 2),
                MakeReadDto(address: 200, dataType: TypeCode.Single, shortOffset: 2),
                MakeReadDto(address: 350, dataType: TypeCode.Double, shortOffset: 4),
                MakeReadDto(address: 360, dataType: TypeCode.Int16, shortOffset: 1)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0].Count);
            Assert.AreEqual(1, result[1].Count);
            Assert.AreEqual(2, result[2].Count);
        }

        [TestMethod]
        public void GroupByAddress_MixedTypesAddressesOutOfOrder_SortsAndGroups()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 300, dataType: TypeCode.Double, shortOffset: 4),
                MakeReadDto(address: 10, dataType: TypeCode.Int16, shortOffset: 1),
                MakeReadDto(address: 310, dataType: TypeCode.Single, shortOffset: 2),
                MakeReadDto(address: 15, dataType: TypeCode.Int32, shortOffset: 2)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(10, result[0][0].Address);
            Assert.AreEqual(15, result[0][1].Address);
            Assert.AreEqual(300, result[1][0].Address);
            Assert.AreEqual(310, result[1][1].Address);
        }

        #endregion

        #region 不同数据类型连续读取场景测试

        [TestMethod]
        public void GroupByAddress_MixedTypesContiguousRegisters_GroupedForEfficientRead()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100, dataType: TypeCode.Int16, shortOffset: 1, length: 2),
                MakeReadDto(address: 102, dataType: TypeCode.Int32, shortOffset: 2, length: 1),
                MakeReadDto(address: 104, dataType: TypeCode.Single, shortOffset: 2, length: 1),
                MakeReadDto(address: 106, dataType: TypeCode.Boolean, shortOffset: 1, length: 1)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(4, result[0].Count);
        }

        [TestMethod]
        public void GroupByAddress_MixedTypesMultiRegister_GroupsAndPreservesMetadata()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 0, dataType: TypeCode.Boolean, shortOffset: 1, length: 1),
                MakeReadDto(address: 1, dataType: TypeCode.Int16, shortOffset: 1, length: 3),
                MakeReadDto(address: 4, dataType: TypeCode.Int32, shortOffset: 2, length: 2),
                MakeReadDto(address: 8, dataType: TypeCode.Single, shortOffset: 2, length: 2),
                MakeReadDto(address: 12, dataType: TypeCode.Double, shortOffset: 4, length: 1)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].Count);
            Assert.AreEqual(TypeCode.Boolean, result[0][0].DataType);
            Assert.AreEqual(TypeCode.Int16, result[0][1].DataType);
            Assert.AreEqual(TypeCode.Int32, result[0][2].DataType);
            Assert.AreEqual(TypeCode.Single, result[0][3].DataType);
            Assert.AreEqual(TypeCode.Double, result[0][4].DataType);
        }

        [TestMethod]
        public void GroupByAddress_StringTypeContinuous_GroupsTogether()
        {
            var items = new List<DevPlcPointReadDto>
            {
                MakeReadDto(address: 100, dataType: TypeCode.String, shortOffset: 1, length: 5),
                MakeReadDto(address: 105, dataType: TypeCode.Int16, shortOffset: 1, length: 1)
            };
            var result = InvokeGroupByAddress(items);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Count);
        }

        #endregion

        #region 辅助方法

        private static DevPlcPointReadDto MakeReadDto(int address, TypeCode dataType = TypeCode.Int16, int shortOffset = 1, int length = 1)
        {
            return new DevPlcPointReadDto
            {
                IpAddress = "127.0.0.1",
                Port = 6000,
                Prefix = "D",
                DataType = dataType,
                Address = address,
                Length = length,
                ShortOffset = shortOffset
            };
        }

        private static List<List<DevPlcPointReadDto>> InvokeGroupByAddress(List<DevPlcPointReadDto> items)
        {
            var mcp = new McpCommunication();
            var method = typeof(McpCommunication).GetMethod(
                "GroupByAddress",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            return (List<List<DevPlcPointReadDto>>)method.Invoke(mcp, new object[] { items });
        }

        #endregion
    }
}
