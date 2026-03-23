import { describe, it, expect, vi, beforeEach } from 'vitest'
import { searchRecipesApi } from '@/lib/api/search'
import type { UnifiedSearchResponse } from '@/types'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

function makeResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as Response
}

const mockSearchResponse: UnifiedSearchResponse = {
  results: [
    {
      id: '1',
      title: 'Spaghetti Carbonara',
      imageUrl: 'https://example.com/img.jpg',
      cuisines: ['Italian'],
      dishTypes: ['main course'],
      readyInMinutes: 30,
      popularity: 100,
      dataSource: 'Spoonacular',
      score: 0.9,
    },
  ],
  metadata: {
    totalResults: 1,
    quotaExhausted: false,
    nextCursor: undefined,
  },
}

describe('searchRecipesApi', () => {
  beforeEach(() => {
    mockFetch.mockClear()
  })

  it('calls GET /api/v1/search/recipes with no params', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockSearchResponse))
    const result = await searchRecipesApi({})
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/search/recipes'),
      expect.objectContaining({ credentials: 'include' })
    )
    expect(result).toEqual(mockSearchResponse)
  })

  it('includes query param q', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockSearchResponse))
    await searchRecipesApi({ q: 'pasta' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('q=pasta'),
      expect.anything()
    )
  })

  it('includes cuisines param', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockSearchResponse))
    await searchRecipesApi({ cuisines: 'Italian,Mexican' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('cuisines=Italian%2CMexican'),
      expect.anything()
    )
  })

  it('includes maxReadyTime param', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockSearchResponse))
    await searchRecipesApi({ maxReadyTime: 30 })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('maxReadyTime=30'),
      expect.anything()
    )
  })

  it('includes cursor param for pagination', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockSearchResponse))
    await searchRecipesApi({ cursor: 'abc123' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('cursor=abc123'),
      expect.anything()
    )
  })

  it('throws ApiErrorData on 503', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({ detail: 'Service unavailable' }, 503))
    await expect(searchRecipesApi({})).rejects.toMatchObject({ status: 503, message: 'Service unavailable' })
  })

  it('throws with generic message on unknown error', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({}, 500))
    await expect(searchRecipesApi({})).rejects.toMatchObject({ status: 500, message: 'An error occurred' })
  })
})
