using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;

namespace SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication
{
    public interface IPlcCommunication
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="readPlcInfo"></param>
        /// <returns></returns>
        Result<DevPlcPointDto> Read(DevPlcPointDto readPlcInfo);

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="readPlcInfo"></param>
        /// <returns></returns>
        Result<List<DevPlcPointDto>> Read(List<DevPlcPointDto> readPlcInfo);

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="devPlcPointMcDto"></param>
        /// <returns></returns>
        Result Write(DevPlcPointDto devPlcPointMcDto);

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="pointMcWriteDto"></param>
        /// <returns></returns>
        Result Write(List<DevPlcPointDto> pointMcWriteDto);
    }
}
