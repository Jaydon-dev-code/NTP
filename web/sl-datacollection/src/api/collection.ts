import { http } from './http'
import type { DeviceDataCollectionInfo, MqttPublishConfig } from './types'

/** 注册设备采集参数 */
export interface DeviceRegisterDto {
  EquipmentId: string
  MqttConfig?: MqttPublishConfig
}

/** 启用/禁用参数 */
export interface DeviceEnabledDto {
  EquipmentId: string
  IsEnabled: boolean
}

export const collectionApi = {
  // 注册 / 注销
  registerDevice: (dto: DeviceRegisterDto) => http.post('DeviceDataCollection/RegisterDevice', dto),
  registerLine: (lineId: number) => http.post('DeviceDataCollection/RegisterLine', lineId),
  registerFactory: (factoryId: number) =>
    http.post('DeviceDataCollection/RegisterFactory', factoryId),
  unregisterDevice: (equipmentId: string) =>
    http.post('DeviceDataCollection/UnregisterDevice', equipmentId),

  // 运行控制
  start: (equipmentId: string) => http.post('DeviceDataCollection/Start', equipmentId),
  stop: (equipmentId: string) => http.post('DeviceDataCollection/Stop', equipmentId),
  restart: (equipmentId: string) => http.post('DeviceDataCollection/Restart', equipmentId),
  setEnabled: (dto: DeviceEnabledDto) => http.post('DeviceDataCollection/SetEnabled', dto),
  startAll: () => http.post('DeviceDataCollection/StartAll', {}),
  stopAll: () => http.post('DeviceDataCollection/StopAll', {}),

  // 查询
  getAllRegistered: () =>
    http.post<DeviceDataCollectionInfo[]>('DeviceDataCollection/GetAllRegistered', {}),
  getRegistered: (equipmentId: string) =>
    http.post<DeviceDataCollectionInfo>('DeviceDataCollection/GetRegistered', equipmentId),
  getDeviceValues: (equipmentId: string) =>
    http.post<Record<string, unknown>>('DeviceDataCollection/GetDeviceValues', equipmentId),

}
