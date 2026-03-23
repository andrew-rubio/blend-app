import type { NotificationsPageResponse, UnreadCountResponse } from '@/types'

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
    } catch { }
    const err: ApiErrorData = { message, status: response.status }
    throw err
  }
  if (response.status === 204) return undefined as unknown as T
  return response.json() as Promise<T>
}

export async function getNotificationsApi(cursor?: string): Promise<NotificationsPageResponse> {
  const url = new URL(`${API_URL}/api/v1/notifications`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<NotificationsPageResponse>(response)
}

export async function getUnreadCountApi(): Promise<UnreadCountResponse> {
  const response = await fetch(`${API_URL}/api/v1/notifications/unread-count`, {
    credentials: 'include',
  })
  return handleResponse<UnreadCountResponse>(response)
}

export async function markNotificationReadApi(id: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/notifications/${encodeURIComponent(id)}/read`,
    { method: 'POST', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export async function markAllNotificationsReadApi(): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/notifications/read-all`, {
    method: 'POST',
    credentials: 'include',
  })
  return handleResponse<void>(response)
}
