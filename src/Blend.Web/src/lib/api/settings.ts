import type { AppSettings, UpdateSettingsRequest } from '@/types'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

interface ApiErrorData {
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

/**
 * Retrieves the current user's app settings.
 * GET /api/v1/settings
 */
export async function getSettingsApi(): Promise<AppSettings> {
  const response = await fetch(`${API_URL}/api/v1/settings`, {
    credentials: 'include',
  })
  return handleResponse<AppSettings>(response)
}

/**
 * Updates the current user's app settings.
 * PUT /api/v1/settings
 */
export async function updateSettingsApi(data: UpdateSettingsRequest): Promise<AppSettings> {
  const response = await fetch(`${API_URL}/api/v1/settings`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AppSettings>(response)
}

/**
 * Requests account deletion (starts 30-day grace period).
 * POST /api/v1/users/me/delete-request
 */
export async function requestAccountDeletionApi(data: { password?: string }): Promise<{ scheduledDeletionDate: string }> {
  const response = await fetch(`${API_URL}/api/v1/users/me/delete-request`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<{ scheduledDeletionDate: string }>(response)
}

/**
 * Cancels a pending account deletion request.
 * POST /api/v1/users/me/cancel-deletion
 */
export async function cancelAccountDeletionApi(): Promise<{ message: string }> {
  const response = await fetch(`${API_URL}/api/v1/users/me/cancel-deletion`, {
    method: 'POST',
    credentials: 'include',
  })
  return handleResponse<{ message: string }>(response)
}
