import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import {
  createSessionApi,
  getActiveSessionApi,
  getSessionApi,
  addIngredientApi,
  removeIngredientApi,
  addDishApi,
  removeDishApi,
  pauseSessionApi,
  completeSessionApi,
  getSuggestionsApi,
  getIngredientDetailApi,
  searchIngredientsApi,
} from '@/lib/api/cookMode'
import type { CookingSession, SessionSuggestionsResult, IngredientDetailResult, IngredientSearchResult } from '@/types'

const mockSession: CookingSession = {
  id: 'session-1',
  userId: 'user-1',
  dishes: [],
  addedIngredients: [],
  status: 'Active',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

const mockFetch = vi.fn()

describe('cookMode API', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', mockFetch)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  function okResponse(data: unknown, status = 200) {
    return Promise.resolve({
      ok: true,
      status,
      json: () => Promise.resolve(data),
    } as Response)
  }

  function errorResponse(status: number, body: unknown) {
    return Promise.resolve({
      ok: false,
      status,
      json: () => Promise.resolve(body),
    } as Response)
  }

  it('createSessionApi POSTs to /cook-sessions', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    const result = await createSessionApi({})
    expect(result).toEqual(mockSession)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions'),
      expect.objectContaining({ method: 'POST' })
    )
  })

  it('getActiveSessionApi GETs /cook-sessions/active', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    const result = await getActiveSessionApi()
    expect(result).toEqual(mockSession)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/active'),
      expect.any(Object)
    )
  })

  it('getSessionApi GETs /cook-sessions/:id', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    const result = await getSessionApi('session-1')
    expect(result).toEqual(mockSession)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1'),
      expect.any(Object)
    )
  })

  it('addIngredientApi POSTs to /cook-sessions/:id/ingredients', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    await addIngredientApi('session-1', { ingredientId: 'ing-1', name: 'Garlic' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/ingredients'),
      expect.objectContaining({ method: 'POST' })
    )
  })

  it('removeIngredientApi DELETEs /cook-sessions/:id/ingredients/:ingId', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    await removeIngredientApi('session-1', 'ing-1')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/ingredients/ing-1'),
      expect.objectContaining({ method: 'DELETE' })
    )
  })

  it('removeIngredientApi appends dishId query param when provided', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    await removeIngredientApi('session-1', 'ing-1', 'dish-1')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('dishId=dish-1'),
      expect.any(Object)
    )
  })

  it('addDishApi POSTs to /cook-sessions/:id/dishes', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    await addDishApi('session-1', { name: 'Pasta' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/dishes'),
      expect.objectContaining({ method: 'POST' })
    )
  })

  it('removeDishApi DELETEs /cook-sessions/:id/dishes/:dishId', async () => {
    mockFetch.mockReturnValue(okResponse(mockSession))
    await removeDishApi('session-1', 'dish-1')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/dishes/dish-1'),
      expect.objectContaining({ method: 'DELETE' })
    )
  })

  it('pauseSessionApi POSTs to /cook-sessions/:id/pause', async () => {
    mockFetch.mockReturnValue(okResponse({ ...mockSession, status: 'Paused' }))
    const result = await pauseSessionApi('session-1')
    expect(result.status).toBe('Paused')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/pause'),
      expect.objectContaining({ method: 'POST' })
    )
  })

  it('completeSessionApi POSTs to /sessions/:id/complete', async () => {
    mockFetch.mockReturnValue(okResponse({ ...mockSession, status: 'Completed' }))
    const result = await completeSessionApi('session-1')
    expect(result.status).toBe('Completed')
  })

  it('getSuggestionsApi GETs suggestions', async () => {
    const mockResult: SessionSuggestionsResult = { suggestions: [], kbUnavailable: false }
    mockFetch.mockReturnValue(okResponse(mockResult))
    const result = await getSuggestionsApi('session-1')
    expect(result).toEqual(mockResult)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/suggestions'),
      expect.any(Object)
    )
  })

  it('getSuggestionsApi appends dishId and limit', async () => {
    mockFetch.mockReturnValue(okResponse({ suggestions: [], kbUnavailable: false }))
    await getSuggestionsApi('session-1', 'dish-1', 5)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('dishId=dish-1'),
      expect.any(Object)
    )
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('limit=5'),
      expect.any(Object)
    )
  })

  it('getIngredientDetailApi GETs ingredient detail', async () => {
    const mockDetail: IngredientDetailResult = {
      ingredientId: 'ing-1',
      name: 'Garlic',
      substitutes: [],
    }
    mockFetch.mockReturnValue(okResponse(mockDetail))
    const result = await getIngredientDetailApi('session-1', 'ing-1')
    expect(result.name).toBe('Garlic')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/cook-sessions/session-1/ingredients/ing-1/detail'),
      expect.any(Object)
    )
  })

  it('searchIngredientsApi searches ingredients', async () => {
    const mockResults: IngredientSearchResult[] = [{ id: 'ing-1', name: 'Garlic' }]
    mockFetch.mockReturnValue(okResponse(mockResults))
    const result = await searchIngredientsApi('gar')
    expect(result).toEqual(mockResults)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('q=gar'),
      expect.any(Object)
    )
  })

  it('handleResponse throws ApiErrorData on non-ok response with detail', async () => {
    mockFetch.mockReturnValue(errorResponse(404, { detail: 'Not found' }))
    await expect(getSessionApi('missing')).rejects.toMatchObject({ message: 'Not found', status: 404 })
  })

  it('handleResponse throws ApiErrorData on non-ok response with message', async () => {
    mockFetch.mockReturnValue(errorResponse(500, { message: 'Server error' }))
    await expect(getSessionApi('x')).rejects.toMatchObject({ message: 'Server error', status: 500 })
  })

  it('handleResponse returns undefined for 204', async () => {
    mockFetch.mockReturnValue(Promise.resolve({ ok: true, status: 204, json: () => Promise.resolve(null) } as Response))
    const result = await pauseSessionApi('session-1')
    expect(result).toBeUndefined()
  })
})
