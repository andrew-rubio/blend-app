import type { MyProfile, PublicProfile, UpdateProfileRequest, ProfileRecipesResponse } from '@/types'

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

export async function getMyProfileApi(): Promise<MyProfile> {
  const response = await fetch(`${API_URL}/api/v1/users/me/profile`, {
    credentials: 'include',
  })
  return handleResponse<MyProfile>(response)
}

export async function updateMyProfileApi(data: UpdateProfileRequest): Promise<MyProfile> {
  const response = await fetch(`${API_URL}/api/v1/users/me/profile`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse<MyProfile>(response)
}

export async function getPublicProfileApi(userId: string): Promise<PublicProfile> {
  const response = await fetch(`${API_URL}/api/v1/users/${encodeURIComponent(userId)}/profile`, {
    credentials: 'include',
  })
  return handleResponse<PublicProfile>(response)
}

export async function getMyRecipesApi(cursor?: string): Promise<ProfileRecipesResponse> {
  const url = new URL(`${API_URL}/api/v1/users/me/recipes`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<ProfileRecipesResponse>(response)
}

export async function getLikedRecipesApi(cursor?: string): Promise<ProfileRecipesResponse> {
  const url = new URL(`${API_URL}/api/v1/users/me/liked-recipes`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<ProfileRecipesResponse>(response)
}

export async function getPublicUserRecipesApi(userId: string, cursor?: string): Promise<ProfileRecipesResponse> {
  const url = new URL(`${API_URL}/api/v1/users/${encodeURIComponent(userId)}/recipes`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<ProfileRecipesResponse>(response)
}

export async function toggleRecipeVisibilityApi(recipeId: string, isPublic: boolean): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/recipes/${encodeURIComponent(recipeId)}`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ isPublic }),
  })
  return handleResponse<void>(response)
}

export async function deleteRecipeApi(recipeId: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/recipes/${encodeURIComponent(recipeId)}?confirm=true`,
    { method: 'DELETE', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export interface UploadUrlRequest {
  contentType: string
  fileSizeBytes: number
  uploadUse: 'Profile'
}

export interface UploadUrlResponse {
  sasUrl: string
  blobPath: string
  expiresAt: string
}

export interface UploadCompleteRequest {
  blobPath: string
  uploadUse: 'Profile'
}

export interface UploadCompleteResponse {
  mediaUrl: string
  processingPending: boolean
}

export async function getUploadUrlApi(data: UploadUrlRequest): Promise<UploadUrlResponse> {
  const response = await fetch(`${API_URL}/api/v1/media/upload-url`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse<UploadUrlResponse>(response)
}

export async function uploadFileToSasUrl(sasUrl: string, file: File): Promise<void> {
  const response = await fetch(sasUrl, {
    method: 'PUT',
    headers: { 'x-ms-blob-type': 'BlockBlob', 'Content-Type': file.type },
    body: file,
  })
  if (!response.ok) {
    throw { message: 'File upload failed', status: response.status } satisfies ApiErrorData
  }
}

export async function completeUploadApi(data: UploadCompleteRequest): Promise<UploadCompleteResponse> {
  const response = await fetch(`${API_URL}/api/v1/media/upload-complete`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse<UploadCompleteResponse>(response)
}
