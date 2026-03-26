import type {
  FriendsPageResponse,
  FriendRequestsPageResponse,
  UserSearchPageResponse,
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
    } catch { }
    const err: ApiErrorData = { message, status: response.status }
    throw err
  }
  if (response.status === 204) return undefined as unknown as T
  return response.json() as Promise<T>
}

export async function getFriendsApi(cursor?: string): Promise<FriendsPageResponse> {
  const url = new URL(`${API_URL}/api/v1/friends`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<FriendsPageResponse>(response)
}

export async function getIncomingRequestsApi(cursor?: string): Promise<FriendRequestsPageResponse> {
  const url = new URL(`${API_URL}/api/v1/friends/requests`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<FriendRequestsPageResponse>(response)
}

export async function getSentRequestsApi(cursor?: string): Promise<FriendRequestsPageResponse> {
  const url = new URL(`${API_URL}/api/v1/friends/requests/sent`)
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<FriendRequestsPageResponse>(response)
}

export async function sendFriendRequestApi(targetUserId: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/v1/friends/requests`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ targetUserId }),
  })
  return handleResponse<void>(response)
}

export async function acceptFriendRequestApi(requestId: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/friends/requests/${encodeURIComponent(requestId)}/accept`,
    { method: 'POST', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export async function declineFriendRequestApi(requestId: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/friends/requests/${encodeURIComponent(requestId)}`,
    { method: 'DELETE', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export async function removeFriendApi(friendUserId: string): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/v1/friends/${encodeURIComponent(friendUserId)}`,
    { method: 'DELETE', credentials: 'include' }
  )
  return handleResponse<void>(response)
}

export async function searchUsersApi(
  query: string,
  cursor?: string,
  pageSize = 20
): Promise<UserSearchPageResponse> {
  const url = new URL(`${API_URL}/api/v1/users/search`)
  url.searchParams.set('q', query)
  url.searchParams.set('pageSize', String(pageSize))
  if (cursor) url.searchParams.set('cursor', cursor)
  const response = await fetch(url.toString(), { credentials: 'include' })
  return handleResponse<UserSearchPageResponse>(response)
}
