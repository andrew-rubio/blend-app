import type { Recipe } from '@/types'

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
 * Fetches the full recipe detail.
 * GET /api/v1/recipes/{id}
 */
export async function getRecipeApi(id: string): Promise<Recipe> {
  const response = await fetch(`${API_URL}/api/v1/recipes/${encodeURIComponent(id)}`, {
    credentials: 'include',
  })
  return handleResponse<Recipe>(response)
}

/**
 * Records a view event for "recently viewed" tracking.
 * POST /api/v1/recipes/{id}/view
 */
export async function recordViewApi(id: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/recipes/${encodeURIComponent(id)}/view`,
    { method: 'POST', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

/**
 * Likes a recipe for the current user.
 * POST /api/v1/recipes/{id}/like
 */
export async function likeRecipeApi(id: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/recipes/${encodeURIComponent(id)}/like`,
    { method: 'POST', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

/**
 * Unlikes a recipe for the current user.
 * DELETE /api/v1/recipes/{id}/like
 */
export async function unlikeRecipeApi(id: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/recipes/${encodeURIComponent(id)}/like`,
    { method: 'DELETE', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export interface UpdateRecipePayload {
  title: string
  description?: string
  ingredients: { quantity: number; unit: string; ingredientName: string; ingredientId?: string }[]
  directions: { stepNumber: number; text: string; mediaUrl?: string }[]
  prepTime?: number
  cookTime?: number
  servings?: number
  cuisineType?: string
  dishType?: string
  tags?: string[]
  featuredPhotoUrl?: string
  photos?: string[]
  isPublic?: boolean
}

/**
 * Updates a community recipe.
 * PUT /api/v1/recipes/{id}
 */
export async function updateRecipeApi(id: string, payload: UpdateRecipePayload): Promise<Recipe> {
  const response = await fetch(`${API_URL}/api/v1/recipes/${encodeURIComponent(id)}`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return handleResponse<Recipe>(response)
}
