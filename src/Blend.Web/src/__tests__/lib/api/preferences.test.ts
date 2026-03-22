import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getPreferencesApi,
  updatePreferencesApi,
  getCuisinesApi,
  getDishTypesApi,
  getDietsApi,
  getIntolerancesApi,
} from '@/lib/api/preferences'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

function makeResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as Response
}

describe('preferences API client', () => {
  beforeEach(() => {
    mockFetch.mockClear()
  })

  describe('getPreferencesApi', () => {
    it('calls GET /api/v1/users/me/preferences with credentials', async () => {
      const mockPrefs = {
        favoriteCuisines: ['Italian'],
        favoriteDishTypes: [],
        diets: [],
        intolerances: [],
        dislikedIngredientIds: [],
      }
      mockFetch.mockResolvedValueOnce(makeResponse(mockPrefs))
      const result = await getPreferencesApi()
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/users/me/preferences'),
        expect.objectContaining({ credentials: 'include' })
      )
      expect(result).toEqual(mockPrefs)
    })

    it('throws ApiErrorData on 401', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse({ detail: 'Unauthorized' }, 401))
      await expect(getPreferencesApi()).rejects.toMatchObject({ status: 401, message: 'Unauthorized' })
    })
  })

  describe('updatePreferencesApi', () => {
    it('calls PUT /api/v1/users/me/preferences with payload', async () => {
      const payload = {
        favoriteCuisines: ['Japanese'],
        favoriteDishTypes: ['dessert'],
        diets: ['vegan'],
        intolerances: ['dairy'],
        dislikedIngredientIds: ['cilantro'],
      }
      const mockResponse = { ...payload }
      mockFetch.mockResolvedValueOnce(makeResponse(mockResponse))
      const result = await updatePreferencesApi(payload)
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/users/me/preferences'),
        expect.objectContaining({
          method: 'PUT',
          credentials: 'include',
          body: JSON.stringify(payload),
        })
      )
      expect(result).toEqual(mockResponse)
    })

    it('throws ApiErrorData on 400', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse({ detail: 'Validation failed' }, 400))
      await expect(
        updatePreferencesApi({
          favoriteCuisines: [],
          favoriteDishTypes: [],
          diets: [],
          intolerances: [],
          dislikedIngredientIds: [],
        })
      ).rejects.toMatchObject({ status: 400, message: 'Validation failed' })
    })
  })

  describe('reference list endpoints', () => {
    it('getCuisinesApi calls GET /api/v1/preferences/cuisines', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse(['Italian', 'Japanese']))
      const result = await getCuisinesApi()
      expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining('/api/v1/preferences/cuisines'))
      expect(result).toEqual(['Italian', 'Japanese'])
    })

    it('getDishTypesApi calls GET /api/v1/preferences/dish-types', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse(['dessert', 'main course']))
      const result = await getDishTypesApi()
      expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining('/api/v1/preferences/dish-types'))
      expect(result).toEqual(['dessert', 'main course'])
    })

    it('getDietsApi calls GET /api/v1/preferences/diets', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse(['vegan', 'vegetarian']))
      const result = await getDietsApi()
      expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining('/api/v1/preferences/diets'))
      expect(result).toEqual(['vegan', 'vegetarian'])
    })

    it('getIntolerancesApi calls GET /api/v1/preferences/intolerances', async () => {
      mockFetch.mockResolvedValueOnce(makeResponse(['dairy', 'gluten']))
      const result = await getIntolerancesApi()
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/preferences/intolerances')
      )
      expect(result).toEqual(['dairy', 'gluten'])
    })
  })
})
