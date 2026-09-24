// 与后端实体/返回结构对应的类型（后端 WebApi 默认 PascalCase 序列化）

/** 厂 */
export interface TbFactory {
  Id: number
  FactoryCode: string
  FactoryName: string
  MqttHost?: string
  MqttPort?: number
  MqttUsername?: string
  MqttPassword?: string
  Remark?: string
  CreateTime: string
  ProductionLines?: TbProductionLine[]
}

/** 产线 */
export interface TbProductionLine {
  Id: number
  FactoryId: number
  LineCode: string
  LineName: string
  MqttHost?: string
  MqttPort?: number
  MqttUsername?: string
  MqttPassword?: string
  Remark?: string
  CreateTime: string
  Factory?: TbFactory
  Equipments?: TbEquipment[]
}

/** 设备 */
export interface TbEquipment {
  Id: number
  EquipmentId: string
  DeviceName: string
  LineId?: number
  Remark?: string
  CreateTime: string
  ProductionLine?: TbProductionLine
}

/** MQTT 发布配置 */
export interface MqttPublishConfig {
  Host?: string
  Port?: number
  Username?: string
  Password?: string
  Topic?: string
}

/** 采集服务信息（后端 StationCollectionInfo） */
export interface DeviceDataCollectionInfo {
  EquipmentId: string
  DeviceName: string
  FactoryId?: number
  FactoryCode?: string
  FactoryName?: string
  LineId?: number
  LineCode?: string
  LineName?: string
  Topic?: string
  IsRunning: boolean
  IsEnabled: boolean
  Status: string
  Description?: string
  LatestSnapshot?: Record<string, unknown>
}

/** 服务类别枚举（与后端 ServiceCategory 对应） */
export enum ServiceCategory {
  Client = 0,
  Web = 1,
}

/** 后台采集服务信息（后端 DataCollectionServiceInfo） */
export interface ServiceInfo {
  ServiceId: string
  Name: string
  ServiceType: string
  Category: ServiceCategory
  IsRunning: boolean
  IsEnabled: boolean
  LastUpdateTime: string
  LastOperator: string
  Remark: string
}

// ─── SPC ───

/** SPC 数据类型枚举 */
export enum SPCDataType {
  Numeric = 0,
  Discrete = 1,
}

/** SPC 数据来源枚举 */
export enum SPCDataSource {
  Database = 0,
  PLC = 1,
}

/** SPC 产品 */
export interface SpcProduct {
  Id: number
  ModelNo: string
  Name: string
  Remark?: string
  CreateTime?: string
}

/** SPC 零件 */
export interface SpcPart {
  Id: number
  ModelNo: string
  Name: string
  ProductId?: number
  PartCategory?: string
  Remark?: string
  CreateTime?: string
  Product?: SpcProduct
}

/** SPC 设备 */
export interface SpcEquipment {
  Id: number
  Name: string
  ProductionLine?: string
  ProcessNo?: string
  ProcessName?: string
  Remark?: string
  CreateTime?: string
}

/** SPC 特性 */
export interface SpcCharacteristic {
  Id: number
  Name: string
  CharacteristicNo: string
  DataType: SPCDataType
  InspectionQty: number
  CollectionMethod: string
  DataSource: SPCDataSource
  ProductId?: number | null
  PartId?: number | null
  PartCategory?: string | null
  EquipmentId: number
  Remark?: string
  CreateTime?: string
  Product?: SpcProduct
  Part?: SpcPart
  Equipment?: SpcEquipment
}

/** 当前生产型号内存快照（无本地缓存表，由三方轮询维护） */
export interface SpcCurrentModel {
  ModelNo?: string
  SyncStatus: string
  LastError?: string
  LastSyncTime?: string
}

/** SPC 运行时解析结果（型号→产品→零件） */
export interface SPCResolvedContext {
  ModelNo?: string
  Product?: SpcProduct
  Part?: SpcPart
  PartCategory?: string
  Reason?: string
  IsSuccess: boolean
}

/** SPC 数据库采集源 */
export interface SpcCollectionSource {
  Id: number
  CharacteristicId: number
  ConnectionString?: string
  TableName?: string
  MeasureValueField?: string
  MeasureResultField?: string
  TimeFieldName?: string
  LastCollectionTime?: string
  CreateTime?: string
}

/** SPC PLC 点位（PlcPointId>0=引用已有点位；=0=手动录入用内联字段） */
export interface SpcPlcPoint {
  Id: number
  CharacteristicId: number
  PlcPointId: number
  // 手动录入内联字段
  IpAddress?: string
  Port?: number
  PlcType?: string
  PointName?: string
  Area?: string
  Address?: string
  DataType?: string
  Length?: number
  CreateTime: string
}

/** SPC 采集任务信息（后端 SPCDataCollectionInfo） */
export interface SPCDataCollectionInfo {
  CharacteristicId: number
  FeatureName: string
  DataSource: SPCDataSource
  Description: string
  IsRunning: boolean
  IsEnabled: boolean
  IntervalMs: number
  ResolvedModelNo?: string
  ResolvedPartName?: string
  ResolvedPartModelNo?: string
  ResolveReason?: string
}

/** 采集方式信息（后端 CollectionMethodInfo） */
export interface CollectionMethodInfo {
  ClassName: string
  Description: string
  DataSource: SPCDataSource
}

/** 已有 PLC 点位（来自 Tb_PlcPoint，用于导入选择） */
export interface TbPlcPoint {
  Id: number
  PlcConnectionId: number
  PointName: string
  Description?: string
  FunctionType?: string
  Area: string
  Address: string
  DataType: string
  Length: number
  Remark?: string
}
