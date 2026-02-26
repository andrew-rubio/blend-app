import type { Recipe } from '@/types/recipe'

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5000'

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

export async function getRecipe(id: string): Promise<Recipe> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${id}`)
  if (!res.ok) {
    throw new ApiError(res.status, `Failed to fetch recipe: ${res.statusText}`)
  }
  return res.json() as Promise<Recipe>
}

export async function recordView(id: string): Promise<void> {
  await fetch(`${API_BASE}/api/v1/recipes/${id}/view`, { method: 'POST' })
}

export async function toggleLike(id: string, liked: boolean): Promise<void> {
  const method = liked ? 'POST' : 'DELETE'
  const res = await fetch(`${API_BASE}/api/v1/recipes/${id}/like`, { method })
  if (!res.ok) {
    throw new ApiError(res.status, `Failed to toggle like: ${res.statusText}`)
  }
}
