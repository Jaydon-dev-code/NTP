using McpXLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class DevPlcPointReadDto
    {
        public  DevPlcPointReadDto()
            {}
        public DevPlcPointReadDto(DevPlcPointDto devPlcPointMcDto,int shortOffset)
        {
            IpAddress = devPlcPointMcDto.IpAddress;
            Port = devPlcPointMcDto.Port;
            Prefix = devPlcPointMcDto.Prefix;
            DataType = devPlcPointMcDto.DataType;
            Address = devPlcPointMcDto.Address;
            Length = devPlcPointMcDto.Length;
            ShortOffset = shortOffset;
        }

        public string IpAddress { get;set; }
        public int Port { get;set; }
        public string Prefix { get;set; }
        public TypeCode DataType { get;set; }
        public string Address { get;set; }
        public int Length { get;set; }

        /// <summary>
        /// 对于short 的偏移长度
        /// </summary>
        public int ShortOffset { get;set; }
        public List<object> Value { get; set; }
    }
}
