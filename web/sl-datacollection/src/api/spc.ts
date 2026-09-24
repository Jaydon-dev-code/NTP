import { http } from './http'
import type { ApiResult } from './http'
import type { SpcProduct, SpcPart, SpcEquipment, SpcCharacteristic, SpcCollectionSource, SpcPlcPoint, TbPlcPoint, SPCDataCollectionInfo, CollectionMethodInfo, SpcCurrentModel, SPCResolvedContext } from './types'

export const spcApi = {
  // ─── 产品 ───
  getProducts: () => http.post<SpcProduct[]>('Spc/GetProducts', {}),
  addProduct: (data: SpcProduct) => http.post('Spc/AddProduct', data),
  updateProduct: (data: SpcProduct) => http.post('Spc/UpdateProduct', data),
  deleteProduct: (id: number) => http.post('Spc/DeleteProduct', id),

  // ─── 零件 ───
  getParts: () => http.post<SpcPart[]>('Spc/GetParts', {}),
  getPartsByProduct: (productId: number) =>
    http.post<SpcPart[]>('Spc/GetPartsByProduct', productId),
  addPart: (data: SpcPart) => http.post('Spc/AddPart', data),
  updatePart: (data: SpcPart) => http.post('Spc/UpdatePart', data),
  deletePart: (id: number) => http.post('Spc/DeletePart', id),

  // ─── 设备 ───
  getEquipments: () => http.post<SpcEquipment[]>('Spc/GetEquipments', {}),
  addEquipment: (data: SpcEquipment) => http.post('Spc/AddEquipment', data),
  updateEquipment: (data: SpcEquipment) => http.post('Spc/UpdateEquipment', data),
  deleteEquipment: (id: number) => http.post('Spc/DeleteEquipment', id),

  // ─── 特性 ───
  getCharacteristics: () => http.post<SpcCharacteristic[]>('Spc/GetCharacteristics', {}),
  addCharacteristic: (data: SpcCharacteristic) => http.post('Spc/AddCharacteristic', data),
  updateCharacteristic: (data: SpcCharacteristic) => http.post('Spc/UpdateCharacteristic', data),
  deleteCharacteristic: (id: number) => http.post('Spc/DeleteCharacteristic', id),

  // ─── 数据库采集源 ───
  getCollectionSources: () => http.post<SpcCollectionSource[]>('Spc/GetCollectionSources', {}),
  getCollectionSource: (characteristicId: number) =>
    http.post<SpcCollectionSource>('Spc/GetCollectionSource', characteristicId),
  updateCollectionSource: (data: SpcCollectionSource) =>
    http.post('Spc/UpdateCollectionSource', data),

  // ─── PLC 点位 ───
  getPlcPoints: () => http.post<SpcPlcPoint[]>('Spc/GetPlcPoints', {}),
  getPlcPoint: (characteristicId: number) =>
    http.post<SpcPlcPoint>('Spc/GetPlcPoint', characteristicId),
  getPlcPointsByCharacteristic: (characteristicId: number) =>
    http.post<SpcPlcPoint[]>('Spc/GetPlcPointsByCharacteristic', characteristicId),
  updatePlcPoint: (data: SpcPlcPoint) => http.post('Spc/UpdatePlcPoint', data),
  batchInsertPlcPoints: (list: SpcPlcPoint[]) =>
    http.post('Spc/BatchInsertPlcPoints', list),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  deletePlcPoint: (id: number) => http.post('Spc/DeletePlcPoint', id),
  importPlcPoints: (characteristicId: number, file: File) => {
    const reader = new FileReader()
    return new Promise<ApiResult<null>>((resolve) => {
      reader.onload = () => {
        const base64 = (reader.result as string).split(',')[1]
        resolve(http.post('Spc/ImportPlcPoints', { CharacteristicId: characteristicId, FileBytes: base64 }))
      }
      reader.onerror = () => resolve({ Code: 0, IsSuccess: false, Message: '文件读取失败', Data: null })
      reader.readAsDataURL(file)
    })
  },
  getEquipmentPlcPoints: (equipmentId: number) =>
    http.post<TbPlcPoint[]>('Spc/GetEquipmentPlcPoints', equipmentId),

  // ─── SPC 采集管理 ───
  getSpcDataCollections: () => http.post<SPCDataCollectionInfo[]>('SpcDataCollection/GetAll', {}),
  startSpcDataCollection: (characteristicId: number) =>
    http.post('SpcDataCollection/Start', characteristicId),
  stopSpcDataCollection: (characteristicId: number) =>
    http.post('SpcDataCollection/Stop', characteristicId),
  setSpcDataCollectionEnabled: (dto: { CharacteristicId: number; IsEnabled: boolean }) =>
    http.post('SpcDataCollection/SetEnabled', dto),
  startAllSpcDataCollections: () => http.post('SpcDataCollection/StartAll', {}),
  stopAllSpcDataCollections: () => http.post('SpcDataCollection/StopAll', {}),
  reloadSpcDataCollections: () => http.post('SpcDataCollection/Reload', {}),

  // ─── 当前型号 ───
  getCurrentModel: () => http.post<SpcCurrentModel>('Spc/GetCurrentModel', {}),
  syncCurrentModelNow: () => http.post('Spc/SyncCurrentModelNow', {}),
  resolveCharacteristic: (characteristicId: number) =>
    http.post<SPCResolvedContext>('Spc/ResolveCharacteristic', characteristicId),

  // ─── 采集方式 ───
  getCollectionMethods: () => http.post<CollectionMethodInfo[]>('Spc/GetCollectionMethods', {}),
}
