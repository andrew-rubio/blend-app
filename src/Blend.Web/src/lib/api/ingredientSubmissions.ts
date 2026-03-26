import type {
  IngredientSubmission,
  IngredientSubmissionsResponse,
  CreateIngredientSubmissionRequest,
  CatalogueIngredient,
  IngredientCatalogueResponse,
} from '@/types'

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
 * Searches the ingredient knowledge base.
 * GET /api/v1/ingredients/search?q={query}
 */
export async function searchIngredientsApi(query: string): Promise<CatalogueIngredient[]> {
  const params = new URLSearchParams({ q: query })
  const response = await fetch(`${API_URL}/api/v1/ingredients/search?${params.toString()}`, {
    credentials: 'include',
  })
  const data = await handleResponse<{ results?: CatalogueIngredient[]; ingredients?: CatalogueIngredient[] }>(response)
  return data.results ?? data.ingredients ?? (data as unknown as CatalogueIngredient[])
}

/**
 * Returns a page of ingredients from the knowledge base catalogue.
 * GET /api/v1/ingredients?cursor={cursor}&pageSize={pageSize}
 */
export async function getIngredientCatalogueApi(
  cursor?: string,
  pageSize = 20
): Promise<IngredientCatalogueResponse> {
  const params = new URLSearchParams({ pageSize: String(pageSize) })
  if (cursor) params.set('cursor', cursor)
  const response = await fetch(`${API_URL}/api/v1/ingredients?${params.toString()}`, {
    credentials: 'include',
  })
  return handleResponse<IngredientCatalogueResponse>(response)
}

/**
 * Submits a new ingredient to the knowledge base.
 * POST /api/v1/ingredients/submissions
 */
export async function createIngredientSubmissionApi(
  data: CreateIngredientSubmissionRequest
): Promise<IngredientSubmission> {
  const response = await fetch(`${API_URL}/api/v1/ingredients/submissions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<IngredientSubmission>(response)
}

/**
 * Returns all ingredient submissions made by the current user.
 * GET /api/v1/ingredients/submissions/mine
 */
export async function getMyIngredientSubmissionsApi(): Promise<IngredientSubmissionsResponse> {
  const response = await fetch(`${API_URL}/api/v1/ingredients/submissions/mine`, {
    credentials: 'include',
  })
  return handleResponse<IngredientSubmissionsResponse>(response)
}
