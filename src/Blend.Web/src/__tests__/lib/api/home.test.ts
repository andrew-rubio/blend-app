import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { getHomeApi } from '@/lib/api/home'
import type { HomeResponse } from '@/types'

const mockHomeResponse: HomeResponse = {
  search: { placeholder: 'Search...' },
  featured: { recipes: [], stories: [], videos: [] },
  community: { recipes: [] },
  recentlyViewed: { recipes: [] },
}

describe('getHomeApi', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('calls GET /api/v1/home', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: true,
      status: 200,
      json: async () => mockHomeResponse,
    } as Response)

    const result = await getHomeApi()
    expect(vi.mocked(fetch)).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/home'),
      expect.objectContaining({ credentials: 'include' })
    )
    expect(result).toEqual(mockHomeResponse)
  })

  it('throws ApiErrorData on HTTP error with message', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: false,
      status: 503,
      json: async () => ({ message: 'Service unavailable' }),
    } as Response)

    await expect(getHomeApi()).rejects.toEqual({ message: 'Service unavailable', status: 503 })
  })

  it('throws ApiErrorData on HTTP error with detail', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({ detail: 'Internal error' }),
    } as Response)

    await expect(getHomeApi()).rejects.toEqual({ message: 'Internal error', status: 500 })
  })

  it('throws with generic message on parse failure', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => { throw new Error('parse error') },
    } as unknown as Response)

    await expect(getHomeApi()).rejects.toEqual({ message: 'An error occurred', status: 500 })
  })
})
