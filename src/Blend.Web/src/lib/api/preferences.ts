import type { UserPreferences, UpdatePreferencesRequest } from '@/types'

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
 * Retrieves the current user's saved preferences.
 * GET /api/v1/users/me/preferences
 */
export async function getPreferencesApi(): Promise<UserPreferences> {
  const response = await fetch(`${API_URL}/api/v1/users/me/preferences`, {
    credentials: 'include',
  })
  return handleResponse<UserPreferences>(response)
}

/**
 * Replaces all of the current user's preferences atomically.
 * PUT /api/v1/users/me/preferences
 */
export async function updatePreferencesApi(
  data: UpdatePreferencesRequest
): Promise<UserPreferences> {
  const response = await fetch(`${API_URL}/api/v1/users/me/preferences`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<UserPreferences>(response)
}

/**
 * Returns the list of supported cuisine types.
 * GET /api/v1/preferences/cuisines
 */
export async function getCuisinesApi(): Promise<string[]> {
  const response = await fetch(`${API_URL}/api/v1/preferences/cuisines`)
  return handleResponse<string[]>(response)
}

/**
 * Returns the list of supported dish types.
 * GET /api/v1/preferences/dish-types
 */
export async function getDishTypesApi(): Promise<string[]> {
  const response = await fetch(`${API_URL}/api/v1/preferences/dish-types`)
  return handleResponse<string[]>(response)
}

/**
 * Returns the list of supported dietary plans.
 * GET /api/v1/preferences/diets
 */
export async function getDietsApi(): Promise<string[]> {
  const response = await fetch(`${API_URL}/api/v1/preferences/diets`)
  return handleResponse<string[]>(response)
}

/**
 * Returns the list of supported intolerances.
 * GET /api/v1/preferences/intolerances
 */
export async function getIntolerancesApi(): Promise<string[]> {
  const response = await fetch(`${API_URL}/api/v1/preferences/intolerances`)
  return handleResponse<string[]>(response)
}
