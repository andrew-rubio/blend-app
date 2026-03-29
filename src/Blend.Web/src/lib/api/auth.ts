import type { User } from '@/types'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  displayName: string
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  user: User
}

export interface ApiErrorData {
  message: string
  status: number
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let message = 'An error occurred'
    try {
      const body = (await response.json()) as { message?: string; detail?: string }
      if (body.detail) message = body.detail
      else if (body.message) message = body.message
    } catch {
      // ignore parse errors
    }
    const err: ApiErrorData = { message, status: response.status }
    throw err
  }
  if (response.status === 204) return undefined as unknown as T
  return response.json() as Promise<T>
}

export async function loginApi(data: LoginRequest): Promise<AuthResponse> {
  const response = await fetch(`${API_URL}/api/v1/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AuthResponse>(response)
}

export async function registerApi(data: RegisterRequest): Promise<AuthResponse> {
  const response = await fetch(`${API_URL}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AuthResponse>(response)
}

export async function logoutApi(): Promise<void> {
  await fetch(`${API_URL}/api/v1/auth/logout`, {
    method: 'POST',
    credentials: 'include',
  })
}

export async function refreshTokenApi(): Promise<{ token: string }> {
  const response = await fetch(`${API_URL}/api/v1/auth/refresh`, {
    method: 'POST',
    credentials: 'include',
  })
  return handleResponse<{ token: string }>(response)
}

export async function forgotPasswordApi(email: string): Promise<void> {
  await fetch(`${API_URL}/api/v1/auth/forgot-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  })
}

export async function resetPasswordApi(token: string, password: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/auth/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token, password }),
  })
  return handleResponse<void>(response)
}

export function getSocialLoginUrl(provider: 'google' | 'facebook' | 'twitter'): string {
  return `${API_URL}/api/v1/auth/${provider}`
}
