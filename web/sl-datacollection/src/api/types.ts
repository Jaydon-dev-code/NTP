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
