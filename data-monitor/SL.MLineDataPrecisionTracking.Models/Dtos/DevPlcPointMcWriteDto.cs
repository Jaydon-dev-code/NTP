using McpXLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class DevPlcPointMcWriteDto
    {
        public DevPlcPointMcWriteDto(DevPlcPointMcDto devPlcPointMcDto)
        {
            IpAddress = devPlcPointMcDto.IpAddress;
            Port = devPlcPointMcDto.Port;
            Prefix = devPlcPointMcDto.Prefix;
            DataType = devPlcPointMcDto.DataType;
            Address = devPlcPointMcDto.Address;
            Value= devPlcPointMcDto.Value;
           
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public Prefix Prefix { get; set; }
        public TypeCode DataType { get; set; }
        public int Address { get; set; }  
        public List<object> Value { get; set; }
    }
}
