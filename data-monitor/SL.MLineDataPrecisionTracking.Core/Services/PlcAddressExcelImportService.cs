using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
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
                var list = ReadExcel(excelStream);
                // 按【设备编号+IP】分组导入
                var groups = list.GroupBy(x => new
                {
                    x.EquipmentId,
                    x.DeviceName,
                    x.IpAddress,
                    x.Port,
                });

                foreach (var group in groups)
                {
                    string equipmentId = group.Key.EquipmentId;
                    string deviceName = group.Key.DeviceName;
                    string ip = group.Key.IpAddress;
                    int port = group.Key.Port;

                    var firstRow = group.First();
                  
                    // 1. 按设备编号找设备，存在则删除原有数据（点位、连接、设备）后重新插入
                    var existing = await _equipmentRepository.QueryableFirstAsync(x =>
                        x.EquipmentId == equipmentId
                    );
                    if (existing != null)
                    {
                        await _plcPointRepository.DeleteableAsync(p =>
                            SqlFunc.Subqueryable<Tb_PlcConnection>()
                                .Where(c =>
                                    c.EquipmentId == existing.Id
                                    && c.Id == p.PlcConnectionId
                                )
                                .Any()
                        );
                        await _plcConnectionRepository.DeleteableAsync(c =>
                            c.EquipmentId == existing.Id
                        );
                        await _equipmentRepository.DeleteableAsync(e => e.Id == existing.Id);
                    }

                    // 2. 找设备，没有就新增（设备编号唯一）
                    var device = await _equipmentRepository.QueryableFirstAsync(x =>
                        x.EquipmentId == equipmentId
                    );
                    int deviceId = 0;
                    if (device == null)
                    {
                        device = new Tb_Equipment() { DeviceName = deviceName, EquipmentId = equipmentId };
                        deviceId = await _equipmentRepository.ExecuteReturnIdentityAsync(device);
                    }
                    else
                    {
                        deviceId = device.Id;
                    }

                    // 3. 找PLC连接，没有就新增
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
                          
                        };
                        plcId = await _plcConnectionRepository.ExecuteReturnIdentityAsync(plc);
                    }
                    else
                    {
                        plcId = plc.Id;
                    }

                    // 4. 批量插入点位
                    List<Tb_PlcPoint> points = group
                        .Select(x => new Tb_PlcPoint
                        {
                            PlcConnectionId = plcId,
                            PointName = x.PointName,
                            Description = x.Description == null ? "" : x.Description,
                            FunctionType = x.FunctionType,
                            Area = x.Area,
                            Address = x.Address,
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
                int indenx = 0;
                var dto = new PlcPointImportDto
                {
                    EquipmentId = row.GetCell(indenx++)?.ToString()?.Trim(),
                    DeviceName = row.GetCell(indenx++)?.ToString()?.Trim(),
                    IpAddress = row.GetCell(indenx++)?.ToString()?.Trim(),
                    Port = int.TryParse(row.GetCell(indenx++)?.ToString(), out int p) ? p : 8000,
                    FunctionType = row.GetCell(indenx++)?.ToString()?.Trim() ?? "",
                    PointName = row.GetCell(indenx++)?.ToString()?.Trim(),
                    Description = row.GetCell(indenx++)?.ToString()?.Trim(),
                    Area = row.GetCell(cellnum: indenx++)?.ToString()?.Trim(),
                    Address = row.GetCell(indenx++)?.ToString()?.Trim(),
                    DataType = row.GetCell(indenx++)?.ToString()?.Trim(),
                    Length = int.TryParse(row.GetCell(indenx++)?.ToString(), out int len) ? len : 1,
                    ReadFormula = row.GetCell(indenx++)?.ToString()?.Trim() ?? "",
                    WriteFormula = row.GetCell(indenx++)?.ToString()?.Trim() ?? "",
       
                   
                };

                if (!string.IsNullOrEmpty(dto.PointName) && !string.IsNullOrEmpty(dto.Area))
                    list.Add(dto);
            }

            return list;
        }

     
    }
}
