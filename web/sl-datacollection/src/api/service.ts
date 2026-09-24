import { http } from './http'
import type { ServiceInfo } from './types'

export const serviceApi = {
  // 查询
  getAllServices: () => http.post<ServiceInfo[]>('Service/GetAllServices', {}),
  getService: (id: string) => http.post<ServiceInfo>('Service/ServiceInfo', { id }),
  getWebServices: () => http.post<ServiceInfo[]>('Service/GetWebServices', {}),

  // 单个控制
  startService: (id: string) => http.post('Service/StartService', id),
  stopService: (id: string) => http.post('Service/StopService', id),
  restartService: (id: string) => http.post('Service/RestartService', id),
  setServiceEnabled: (serviceInfo: { ServiceId: string; IsEnabled: boolean }) =>
    http.post('Service/SetServiceEnabled', serviceInfo),

  // 批量控制
  startAllServices: () => http.post('Service/StartAllServices', {}),
  stopAllServices: () => http.post('Service/StopAllServices', {}),
  startAllWebServices: () => http.post('Service/StartAllWebServices', {}),
  stopAllWebServices: () => http.post('Service/StopAllWebServices', {}),
}
