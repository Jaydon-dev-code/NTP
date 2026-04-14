using McpXLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class PlcPointImportDto
    {
        public string DeviceName { get; set; }    // 设备名称
        public string IpAddress { get; set; }     // PLC_IP
        public int Port { get; set; }             // 端口
        public string PointName { get; set; }     // 点位名称
        public string Description { get; set; }   // 描述
        public string Area { get; set; }          // D/M/X/Y
        public int Address { get; set; }          // 地址
        public string DataType { get; set; }      // Int16/Float/Bool
        public int Length { get; set; }
    }
}
