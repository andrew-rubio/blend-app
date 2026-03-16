export interface User {
  id: string
  email: string
  name: string
  role: 'user' | 'admin'
  createdAt: string
}

export interface ApiError {
  message: string
  status: number
  code?: string
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export type ThemeMode = 'light' | 'dark' | 'system'
