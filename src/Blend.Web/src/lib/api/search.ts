import type { SearchQueryParams, UnifiedSearchResponse } from '@/types'

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
  if (response.status === 204) return undefined as unknown as T
  return response.json() as Promise<T>
}

/**
 * Searches recipes via the unified search endpoint.
 * GET /api/v1/search/recipes
 */
export async function searchRecipesApi(params: SearchQueryParams): Promise<UnifiedSearchResponse> {
  const query = new URLSearchParams()
  if (params.q) query.set('q', params.q)
  if (params.cuisines) query.set('cuisines', params.cuisines)
  if (params.diets) query.set('diets', params.diets)
  if (params.dishTypes) query.set('dishTypes', params.dishTypes)
  if (params.maxReadyTime != null) query.set('maxReadyTime', String(params.maxReadyTime))
  if (params.sort) query.set('sort', params.sort)
  if (params.cursor) query.set('cursor', params.cursor)
  if (params.pageSize != null) query.set('pageSize', String(params.pageSize))

  const url = `${API_URL}/api/v1/search/recipes${query.toString() ? `?${query.toString()}` : ''}`
  const response = await fetch(url, { credentials: 'include' })
  return handleResponse<UnifiedSearchResponse>(response)
}
