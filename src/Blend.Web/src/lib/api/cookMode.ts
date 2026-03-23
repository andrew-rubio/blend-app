import type {
  CookingSession,
  CreateCookSessionRequest,
  AddIngredientRequest,
  AddDishRequest,
  SessionSuggestionsResult,
  IngredientDetailResult,
  IngredientSearchResult,
} from '@/types'

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

const BASE = `${API_URL}/api/v1/cook`

export async function createSessionApi(req: CreateCookSessionRequest): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  })
  return handleResponse<CookingSession>(response)
}

export async function getActiveSessionApi(): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/active`, { credentials: 'include' })
  return handleResponse<CookingSession>(response)
}

export async function getSessionApi(id: string): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${id}`, { credentials: 'include' })
  return handleResponse<CookingSession>(response)
}

export async function addIngredientApi(sessionId: string, req: AddIngredientRequest): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/ingredients`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  })
  return handleResponse<CookingSession>(response)
}

export async function removeIngredientApi(sessionId: string, ingredientId: string, dishId?: string): Promise<CookingSession> {
  const query = new URLSearchParams()
  if (dishId) query.set('dishId', dishId)
  const url = `${BASE}/sessions/${sessionId}/ingredients/${ingredientId}${query.toString() ? `?${query.toString()}` : ''}`
  const response = await fetch(url, { method: 'DELETE', credentials: 'include' })
  return handleResponse<CookingSession>(response)
}

export async function addDishApi(sessionId: string, req: AddDishRequest): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/dishes`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  })
  return handleResponse<CookingSession>(response)
}

export async function removeDishApi(sessionId: string, dishId: string): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/dishes/${dishId}`, {
    method: 'DELETE',
    credentials: 'include',
  })
  return handleResponse<CookingSession>(response)
}

export async function pauseSessionApi(sessionId: string): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/pause`, {
    method: 'POST',
    credentials: 'include',
  })
  return handleResponse<CookingSession>(response)
}

export async function completeSessionApi(sessionId: string): Promise<CookingSession> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/complete`, {
    method: 'POST',
    credentials: 'include',
  })
  return handleResponse<CookingSession>(response)
}

export async function getSuggestionsApi(sessionId: string, dishId?: string, limit?: number): Promise<SessionSuggestionsResult> {
  const query = new URLSearchParams()
  if (dishId) query.set('dishId', dishId)
  if (limit != null) query.set('limit', String(limit))
  const url = `${BASE}/sessions/${sessionId}/suggestions${query.toString() ? `?${query.toString()}` : ''}`
  const response = await fetch(url, { credentials: 'include' })
  return handleResponse<SessionSuggestionsResult>(response)
}

export async function getIngredientDetailApi(sessionId: string, ingredientId: string): Promise<IngredientDetailResult> {
  const response = await fetch(`${BASE}/sessions/${sessionId}/ingredients/${ingredientId}/detail`, {
    credentials: 'include',
  })
  return handleResponse<IngredientDetailResult>(response)
}

export async function searchIngredientsApi(q: string, limit?: number): Promise<IngredientSearchResult[]> {
  const query = new URLSearchParams()
  query.set('q', q)
  if (limit != null) query.set('limit', String(limit))
  const response = await fetch(`${API_URL}/api/v1/ingredients/search?${query.toString()}`, {
    credentials: 'include',
  })
  return handleResponse<IngredientSearchResult[]>(response)
}
