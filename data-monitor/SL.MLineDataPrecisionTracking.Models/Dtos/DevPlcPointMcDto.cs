using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McpXLib.Enums;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class DevPlcPointMcDto
    {
        public DevPlcPointMcDto(
            string deviceName,
            string pointName,
            string ipAddress,
            int port,
            Prefix prefix,
            TypeCode typeCode,
            string address,
            int length,
            string readFormula="",
            string writeFormula="",
            List<object> value = null
        )
        {
            DeviceName = deviceName;
            PointName = pointName;
            IpAddress = ipAddress;
            Port = port;
            Prefix = prefix;
            DataType = typeCode;
            Address = address;
            Length = length;
            ReadFormula = readFormula;
            WriteFormula = writeFormula;
            Value = value;
        }

        public string DeviceName { get; set; }
        public string PointName { get; set; }
        public string IpAddress { get; set; } // PLC_IP
        public int Port { get; set; } // 端口
        public Prefix Prefix { get; set; } // D/M/X/Y
        public string Address { get; set; } // 地址
        public TypeCode DataType { get; set; } // Int16/Float/Bool

        public string ReadFormula { get; set; }
        public string WriteFormula { get; set; }
        public int Length { get; set; }

        public List<object> Value { get; set; }
    }
}
