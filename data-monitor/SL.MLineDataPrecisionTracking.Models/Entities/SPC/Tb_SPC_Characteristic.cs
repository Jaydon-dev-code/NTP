using System;
using SqlSugar;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC 特性库 — 定义可检测的产品特性，关联设备与零件类别。
    ///
    /// 绑定关系（型号接入后）：
    ///   - 主绑定：PartCategory（零件类别）— 运行时按 当前型号→产品→类别 找具体零件
    ///   - ProductId/PartId：可选兼容字段；PartId 非空时解析优先直通该零件
    ///   - EquipmentId：仍必填，决定 PLC 点位/采集上下文
    /// </summary>
    [SugarTable("SPC_Characteristic")]
    public class Tb_SPC_Characteristic
    {
        /// <summary>主键</summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>特性名称（采集器 FeatureName 展示用）</summary>
        [SugarColumn(ColumnDescription = "特性名称")]
        public string Name { get; set; }

        /// <summary>特性编号（业务侧唯一编码）</summary>
        [SugarColumn(ColumnDescription = "特性编号")]
        public string CharacteristicNo { get; set; }

        /// <summary>数据类型（数值 / 离散），默认数值</summary>
        [SugarColumn(ColumnDescription = "数据类型")]
        public SPCDataType DataType { get; set; } = SPCDataType.Numeric;

        /// <summary>单次检测数量</summary>
        [SugarColumn(ColumnDescription = "检测数量")]
        public int InspectionQty { get; set; }

        /// <summary>采集方式 = SPCDataCollectionAbstract 子类类名（反射创建）</summary>
        [SugarColumn(ColumnDescription = "采集方式")]
        public string CollectionMethod { get; set; }

        /// <summary>数据来源（数据库 / PLC），决定初始化走 CollectionSource 还是 PlcPoint</summary>
        [SugarColumn(ColumnDescription = "数据来源")]
        public SPCDataSource DataSource { get; set; } = SPCDataSource.Database;

        /// <summary>
        /// 关联产品ID — 可选。
        /// 运行时产品由「当前型号 → SPC_Product.ModelNo」动态解析，此字段主要兼容旧数据/PartId 直通校验。
        /// </summary>
        [SugarColumn(ColumnDescription = "关联产品ID", IsNullable = true)]
        public int? ProductId { get; set; }

        /// <summary>
        /// 关联零件ID — 可选。
        /// 非空时 CurrentModelResolver 优先按此零件直通（旧配置不回归）；
        /// 直通失败则回退到 PartCategory 路径。
        /// </summary>
        [SugarColumn(ColumnDescription = "关联零件ID", IsNullable = true)]
        public int? PartId { get; set; }

        /// <summary>
        /// 零件类别 — 特性的主绑定字段（主数据 SPC_Part.PartCategory 同名）。
        /// 运行时链路：当前型号 → 产品 → 本类别下具体零件。
        /// </summary>
        [SugarColumn(ColumnDescription = "零件类别", IsNullable = true)]
        public string PartCategory { get; set; }

        /// <summary>关联设备ID（必填；PLC 来源时用于筛选可导入点位）</summary>
        [SugarColumn(ColumnDescription = "关联设备ID")]
        public int EquipmentId { get; set; }

        /// <summary>备注</summary>
        [SugarColumn(ColumnDescription = "备注", IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // ── 导航属性（SqlSugar ManyToOne；ProductId/PartId 可空时可能为 null）──
        /// <summary>导航：关联产品（旧绑定，运行时解析结果不写回此处）</summary>
        [Navigate(NavigateType.ManyToOne, nameof(ProductId))]
        public Tb_SPC_Product Product { get; set; }

        /// <summary>导航：关联零件（旧绑定 PartId 直通用）</summary>
        [Navigate(NavigateType.ManyToOne, nameof(PartId))]
        public Tb_SPC_Part Part { get; set; }

        /// <summary>导航：关联设备</summary>
        [Navigate(NavigateType.ManyToOne, nameof(EquipmentId))]
        public Tb_SPC_Equipment Equipment { get; set; }
    }
}
