import type { HomeResponse } from '@/types'

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

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
  return response.json() as Promise<T>
}

/**
 * Fetches all home page sections from the aggregated endpoint.
 * GET /api/v1/home
 */
export async function getHomeApi(): Promise<HomeResponse> {
  const response = await fetch(`${API_URL}/api/v1/home`, {
    credentials: 'include',
  })
  return handleResponse<HomeResponse>(response)
}
