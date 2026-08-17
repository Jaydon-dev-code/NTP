import { http } from './http'
import type { TbEquipment, TbFactory, TbProductionLine } from './types'

/** 设备入线参数 */
export interface EquipmentLineDto {
  EquipmentId: string
  LineId: number
}

export const equipmentApi = {
  // 厂
  getFactories: () => http.post<TbFactory[]>('Equipment/GetFactories', {}),
  addFactory: (data: Partial<TbFactory>) => http.post('Equipment/AddFactory', data),
  updateFactory: (data: Partial<TbFactory>) => http.post('Equipment/UpdateFactory', data),
  deleteFactory: (id: number) => http.post('Equipment/DeleteFactory', id),

  // 产线
  getProductionLines: (factoryId?: number) =>
    http.post<TbProductionLine[]>('Equipment/GetProductionLines', factoryId ?? null),
  addProductionLine: (data: Partial<TbProductionLine>) =>
    http.post('Equipment/AddProductionLine', data),
  updateProductionLine: (data: Partial<TbProductionLine>) =>
    http.post('Equipment/UpdateProductionLine', data),
  deleteProductionLine: (id: number) => http.post('Equipment/DeleteProductionLine', id),

  // 设备
  getEquipments: (factoryId?: number, lineId?: number) =>
    http.post<TbEquipment[]>('Equipment/GetEquipments', { FactoryId: factoryId ?? null, LineId: lineId ?? null }),
  addEquipment: (data: Partial<TbEquipment>) => http.post('Equipment/AddEquipment', data),
  updateEquipment: (data: Partial<TbEquipment>) => http.post('Equipment/UpdateEquipment', data),
  deleteEquipment: (id: string) => http.post('Equipment/DeleteEquipment', id),

  // 入线 / 移出
  addEquipmentToLine: (dto: EquipmentLineDto) => http.post('Equipment/AddEquipmentToLine', dto),
  removeEquipmentFromLine: (id: string) => http.post('Equipment/RemoveEquipmentFromLine', id),

  // 导入点位 Excel（含设备编号、功能分类等 13 列）
  importPlcPoints: (file: File) =>
    http.postFile('DeviceCollectionConfig/ImportPlcPoints', file),
}
