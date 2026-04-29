using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class PlcAddressExcelImportService
    {
        Tb_EquipmentRepository _equipmentRepository;
        Tb_PlcConnectionRepository _plcConnectionRepository;
        Tb_PlcPointRepository _plcPointRepository;

        public PlcAddressExcelImportService(
            Tb_EquipmentRepository equipmentRepository,
            Tb_PlcConnectionRepository plcConnectionRepository,
            Tb_PlcPointRepository plcPointRepository
        )
        {
            _equipmentRepository = equipmentRepository;
            _plcConnectionRepository = plcConnectionRepository;
            _plcPointRepository = plcPointRepository;
        }

        /// <summary>
        /// 导入 PLC 点位 Excel
        /// </summary>
        public async Task<bool> ImportPlcPointsAsync(Stream excelStream)
        {
            try
            {
                await _equipmentRepository.DeleteableAsync(x => true);
                await _plcConnectionRepository.DeleteableAsync(x => true);
                await _plcPointRepository.DeleteableAsync(x => true);
                var list = ReadExcel(excelStream);
                // 按【设备+IP】分组导入
                var groups = list.GroupBy(x => new
                {
                    x.DeviceName,
                    x.IpAddress,
                    x.Port,
                });

                foreach (var group in groups)
                {
                    string deviceName = group.Key.DeviceName;
                    string ip = group.Key.IpAddress;
                    int port = group.Key.Port;

                    // 1. 找设备，没有就新增
                    var device = await _equipmentRepository.QueryableFirstAsync(x =>
                        x.DeviceName == deviceName
                    );
                    int deviceId = 0;
                    if (device == null)
                    {
                        device = new Tb_Equipment() { DeviceName = deviceName };
                        deviceId = await _equipmentRepository.ExecuteReturnIdentityAsync(device);
                    }
                    else
                    {
                        deviceId = device.Id;
                    }

                    // 2. 找PLC连接，没有就新增
                    var plc = await _plcConnectionRepository.QueryableFirstAsync(x =>
                        x.EquipmentId == deviceId && x.IpAddress == ip && x.Port == port
                    );

                    int plcId = 0;
                    if (plc == null)
                    {
                        plc = new Tb_PlcConnection
                        {
                            EquipmentId = deviceId,
                            IpAddress = ip,
                            Port = port,
                            PlcType = "三菱",
                            NetworkType = "MC",
                        };
                        plcId = await _plcConnectionRepository.ExecuteReturnIdentityAsync(plc);
                    }
                    else
                    {
                        plcId = plc.Id;
                    }

                    // 3. 批量插入点位
                    var points = group
                        .Select(x => new Tb_PlcPoint
                        {
                            PlcConnectionId = plcId,
                            PointName = x.PointName,
                            Description = x.Description,
                            Area = x.Area,
                            Address = x.Address.ToString(),
                            DataType = x.DataType,
                            Length = x.Length,
                            WriteFormula = x.WriteFormula,
                            ReadFormula = x.ReadFormula,
                        })
                        .ToList();

                    var insternumber = await _plcPointRepository.InsertableAsync(points);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读取Excel
        /// </summary>
        List<PlcPointImportDto> ReadExcel(Stream stream)
        {
            var list = new List<PlcPointImportDto>();

            IWorkbook workbook = new XSSFWorkbook(stream);
                ISheet sheet = workbook.GetSheetAt(0);

                // 从第2行开始（第1行是表头）
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null)
                        continue;

                    var dto = new PlcPointImportDto
                    {
                        DeviceName = row.GetCell(0)?.ToString()?.Trim(),
                        IpAddress = row.GetCell(1)?.ToString()?.Trim(),
                        Port = int.TryParse(row.GetCell(2)?.ToString(), out int p) ? p : 8000,
                        PointName = row.GetCell(3)?.ToString()?.Trim(),
                        Description = row.GetCell(4)?.ToString()?.Trim(),
                        Area = row.GetCell(5)?.ToString()?.Trim(),
                        Address = int.TryParse(row.GetCell(6)?.ToString(), out int addr) ? addr : 0,
                        DataType = row.GetCell(7)?.ToString()?.Trim(),
                        Length = int.TryParse(row.GetCell(8)?.ToString(), out int len) ? len : 1,
                        ReadFormula = row.GetCell(9)?.ToString()?.Trim()??"",
                        WriteFormula = row.GetCell(10)?.ToString()?.Trim()??"",
                    };

                    if (!string.IsNullOrEmpty(dto.PointName) && !string.IsNullOrEmpty(dto.Area))
                        list.Add(dto);
                }

                return list;
        }
    }
}
