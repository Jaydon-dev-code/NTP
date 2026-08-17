// 基础 HTTP 客户端与通用类型

const BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'http://localhost:59088'

/** 后端统一返回结构 */
export interface ApiResult<T = null> {
  Code: number
  IsSuccess: boolean
  Message: string
  Data: T
}

async function request<T>(url: string, body?: unknown): Promise<ApiResult<T>> {
  const res = await fetch(`${BASE_URL}/api/${url}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  })
  if (!res.ok) {
    return { Code: 0, IsSuccess: false, Message: `HTTP ${res.status}`, Data: null as T }
  }
  return (await res.json()) as ApiResult<T>
}

async function requestFile<T>(url: string, formData: FormData): Promise<ApiResult<T>> {
  const res = await fetch(`${BASE_URL}/api/${url}`, {
    method: 'POST',
    body: formData, // 不手动设 Content-Type，浏览器自动带 multipart boundary
  })
  if (!res.ok) {
    return { Code: 0, IsSuccess: false, Message: `HTTP ${res.status}`, Data: null as T }
  }
  return (await res.json()) as ApiResult<T>
}

export const http = {
  post<T>(url: string, body?: unknown): Promise<ApiResult<T>> {
    return request<T>(url, body)
  },
  postFile<T>(url: string, file: File): Promise<ApiResult<T>> {
    const fd = new FormData()
    fd.append('file', file)
    return requestFile<T>(url, fd)
  },
}
