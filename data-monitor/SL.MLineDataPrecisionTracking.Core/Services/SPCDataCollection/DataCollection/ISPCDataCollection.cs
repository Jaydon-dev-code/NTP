using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection
{
    /// <summary>
    /// SPC 数据采集接口
    /// </summary>
    public interface ISPCDataCollection
    {
        /// <summary>特性名称</summary>
        string FeatureName { get; }

        /// <summary>采集类型（数据库 / PLC）</summary>
        SPCDataSource DataSource { get; }

        /// <summary>描述</summary>
        string Description { get; }

        /// <summary>执行采集</summary>
        void Execute();
    }
}
