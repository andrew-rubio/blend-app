import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getRecipeApi, recordViewApi, likeRecipeApi, unlikeRecipeApi } from '@/lib/api/recipes'
import type { Recipe } from '@/types'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

function makeResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as Response
}

const mockRecipe: Recipe = {
  id: '42',
  title: 'Spaghetti Carbonara',
  description: 'Classic Italian pasta dish.',
  imageUrl: 'https://example.com/carbonara.jpg',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  diets: ['gluten free'],
  intolerances: [],
  servings: 4,
  readyInMinutes: 30,
  prepTimeMinutes: 10,
  cookTimeMinutes: 20,
  difficulty: 'Medium',
  ingredients: [
    { id: 'i1', name: 'pasta', amount: 200, unit: 'g' },
  ],
  steps: [
    { number: 1, step: 'Boil pasta.' },
  ],
  dataSource: 'Spoonacular',
  likeCount: 100,
  isLiked: false,
}

describe('getRecipeApi', () => {
  beforeEach(() => mockFetch.mockClear())

  it('calls GET /api/v1/recipes/{id} with credentials', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockRecipe))
    const result = await getRecipeApi('42')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/recipes/42'),
      expect.objectContaining({ credentials: 'include' })
    )
    expect(result).toEqual(mockRecipe)
  })

  it('throws ApiErrorData on 404', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({ detail: 'Recipe not found' }, 404))
    await expect(getRecipeApi('99')).rejects.toMatchObject({ status: 404, message: 'Recipe not found' })
  })

  it('throws ApiErrorData on 403', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({ message: 'Forbidden' }, 403))
    await expect(getRecipeApi('1')).rejects.toMatchObject({ status: 403, message: 'Forbidden' })
  })

  it('throws with generic message on 500', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({}, 500))
    await expect(getRecipeApi('1')).rejects.toMatchObject({ status: 500, message: 'An error occurred' })
  })

  it('URL-encodes the recipe id', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(mockRecipe))
    await getRecipeApi('hello world')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('hello%20world'),
      expect.anything()
    )
  })
})

describe('recordViewApi', () => {
  beforeEach(() => mockFetch.mockClear())

  it('calls POST /api/v1/recipes/{id}/view', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(null, 204))
    await recordViewApi('42')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/recipes/42/view'),
      expect.objectContaining({ method: 'POST', credentials: 'include' })
    )
  })

  it('silently ignores non-throw errors in calling code', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({ detail: 'error' }, 500))
    await expect(recordViewApi('42')).rejects.toMatchObject({ status: 500 })
  })
})

describe('likeRecipeApi', () => {
  beforeEach(() => mockFetch.mockClear())

  it('calls POST /api/v1/recipes/{id}/like', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(null, 204))
    await likeRecipeApi('42')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/recipes/42/like'),
      expect.objectContaining({ method: 'POST', credentials: 'include' })
    )
  })
})

describe('unlikeRecipeApi', () => {
  beforeEach(() => mockFetch.mockClear())

  it('calls DELETE /api/v1/recipes/{id}/like', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(null, 204))
    await unlikeRecipeApi('42')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/recipes/42/like'),
      expect.objectContaining({ method: 'DELETE', credentials: 'include' })
    )
  })
})
