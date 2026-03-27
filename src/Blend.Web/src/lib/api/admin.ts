import type {
  AdminFeaturedRecipe,
  CreateFeaturedRecipeRequest,
  UpdateFeaturedRecipeRequest,
  AdminStory,
  CreateStoryRequest,
  UpdateStoryRequest,
  AdminVideo,
  CreateVideoRequest,
  UpdateVideoRequest,
  AdminIngredientSubmissionsResponse,
  ApproveSubmissionRequest,
  RejectSubmissionRequest,
  BatchActionRequest,
  AdminDashboardCounts,
  IngredientSubmissionStatus,
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

// ── Dashboard ──────────────────────────────────────────────────────────────────

/**
 * Fetches overview counts for the admin dashboard.
 * Aggregates from multiple endpoints.
 */
export async function getAdminDashboardCountsApi(): Promise<AdminDashboardCounts> {
  const [recipesRes, storiesRes, videosRes, submissionsRes] = await Promise.all([
    fetch(`${API_URL}/api/v1/admin/content/featured-recipes`, { credentials: 'include' }),
    fetch(`${API_URL}/api/v1/admin/content/stories`, { credentials: 'include' }),
    fetch(`${API_URL}/api/v1/admin/content/videos`, { credentials: 'include' }),
    fetch(`${API_URL}/api/v1/admin/ingredients/submissions?status=pending&pageSize=1`, {
      credentials: 'include',
    }),
  ])

  const recipes = await handleResponse<{ items?: AdminFeaturedRecipe[]; data?: AdminFeaturedRecipe[] }>(recipesRes)
  const stories = await handleResponse<{ items?: AdminStory[]; data?: AdminStory[] }>(storiesRes)
  const videos = await handleResponse<{ items?: AdminVideo[]; data?: AdminVideo[] }>(videosRes)
  const submissions = await handleResponse<AdminIngredientSubmissionsResponse>(submissionsRes)

  return {
    featuredRecipes: (recipes.items ?? recipes.data ?? []).length,
    stories: (stories.items ?? stories.data ?? []).length,
    videos: (videos.items ?? videos.data ?? []).length,
    pendingSubmissions: submissions.total ?? 0,
  }
}

// ── Featured Recipes ──────────────────────────────────────────────────────────

/**
 * Returns all featured recipes.
 * GET /api/v1/admin/content/featured-recipes
 */
export async function getFeaturedRecipesApi(): Promise<AdminFeaturedRecipe[]> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/featured-recipes`, {
    credentials: 'include',
  })
  const data = await handleResponse<{ items?: AdminFeaturedRecipe[] } | AdminFeaturedRecipe[]>(response)
  return Array.isArray(data) ? data : (data.items ?? [])
}

/**
 * Creates a new featured recipe entry.
 * POST /api/v1/admin/content/featured-recipes
 */
export async function createFeaturedRecipeApi(
  data: CreateFeaturedRecipeRequest
): Promise<AdminFeaturedRecipe> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/featured-recipes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminFeaturedRecipe>(response)
}

/**
 * Updates an existing featured recipe.
 * PUT /api/v1/admin/content/featured-recipes/{id}
 */
export async function updateFeaturedRecipeApi(
  id: string,
  data: UpdateFeaturedRecipeRequest
): Promise<AdminFeaturedRecipe> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/featured-recipes/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminFeaturedRecipe>(response)
}

/**
 * Deletes a featured recipe.
 * DELETE /api/v1/admin/content/featured-recipes/{id}
 */
export async function deleteFeaturedRecipeApi(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/featured-recipes/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })
  return handleResponse<void>(response)
}

// ── Stories ────────────────────────────────────────────────────────────────────

/**
 * Returns all stories.
 * GET /api/v1/admin/content/stories
 */
export async function getStoriesApi(): Promise<AdminStory[]> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/stories`, {
    credentials: 'include',
  })
  const data = await handleResponse<{ items?: AdminStory[] } | AdminStory[]>(response)
  return Array.isArray(data) ? data : (data.items ?? [])
}

/**
 * Creates a new story.
 * POST /api/v1/admin/content/stories
 */
export async function createStoryApi(data: CreateStoryRequest): Promise<AdminStory> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/stories`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminStory>(response)
}

/**
 * Updates an existing story.
 * PUT /api/v1/admin/content/stories/{id}
 */
export async function updateStoryApi(id: string, data: UpdateStoryRequest): Promise<AdminStory> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/stories/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminStory>(response)
}

/**
 * Deletes a story.
 * DELETE /api/v1/admin/content/stories/{id}
 */
export async function deleteStoryApi(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/stories/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })
  return handleResponse<void>(response)
}

// ── Videos ────────────────────────────────────────────────────────────────────

/**
 * Returns all videos.
 * GET /api/v1/admin/content/videos
 */
export async function getVideosApi(): Promise<AdminVideo[]> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/videos`, {
    credentials: 'include',
  })
  const data = await handleResponse<{ items?: AdminVideo[] } | AdminVideo[]>(response)
  return Array.isArray(data) ? data : (data.items ?? [])
}

/**
 * Creates a new video.
 * POST /api/v1/admin/content/videos
 */
export async function createVideoApi(data: CreateVideoRequest): Promise<AdminVideo> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/videos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminVideo>(response)
}

/**
 * Updates an existing video.
 * PUT /api/v1/admin/content/videos/{id}
 */
export async function updateVideoApi(id: string, data: UpdateVideoRequest): Promise<AdminVideo> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/videos/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(data),
  })
  return handleResponse<AdminVideo>(response)
}

/**
 * Deletes a video.
 * DELETE /api/v1/admin/content/videos/{id}
 */
export async function deleteVideoApi(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/admin/content/videos/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })
  return handleResponse<void>(response)
}

// ── Ingredient Submissions ────────────────────────────────────────────────────

/**
 * Returns ingredient submissions filtered by status.
 * GET /api/v1/admin/ingredients/submissions?status={status}&page={page}&pageSize={pageSize}
 */
export async function getAdminSubmissionsApi(
  status?: IngredientSubmissionStatus,
  page = 1,
  pageSize = 20
): Promise<AdminIngredientSubmissionsResponse> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (status) params.set('status', status)
  const response = await fetch(
    `${API_URL}/api/v1/admin/ingredients/submissions?${params.toString()}`,
    { credentials: 'include' }
  )
  return handleResponse<AdminIngredientSubmissionsResponse>(response)
}

/**
 * Approves an ingredient submission.
 * POST /api/v1/admin/ingredients/submissions/{id}/approve
 */
export async function approveSubmissionApi(
  id: string,
  data?: ApproveSubmissionRequest
): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/admin/ingredients/submissions/${id}/approve`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data ?? {}),
    }
  )
  return handleResponse<void>(response)
}

/**
 * Rejects an ingredient submission.
 * POST /api/v1/admin/ingredients/submissions/{id}/reject
 */
export async function rejectSubmissionApi(
  id: string,
  data?: RejectSubmissionRequest
): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/admin/ingredients/submissions/${id}/reject`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data ?? {}),
    }
  )
  return handleResponse<void>(response)
}

/**
 * Batch approves ingredient submissions.
 * POST /api/v1/admin/ingredients/submissions/batch-approve
 */
export async function batchApproveSubmissionsApi(data: BatchActionRequest): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/admin/ingredients/submissions/batch-approve`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data),
    }
  )
  return handleResponse<void>(response)
}

/**
 * Batch rejects ingredient submissions.
 * POST /api/v1/admin/ingredients/submissions/batch-reject
 */
export async function batchRejectSubmissionsApi(data: BatchActionRequest): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/admin/ingredients/submissions/batch-reject`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data),
    }
  )
  return handleResponse<void>(response)
}
