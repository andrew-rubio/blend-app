import { describe, it, expect, vi, beforeEach } from 'vitest'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

function makeResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as Response
}

function makeSearchResponse(
  results: { id: string; createdAt?: string }[],
  nextCursor?: string
) {
  return {
    results,
    metadata: { nextCursor },
  }
}

describe('sitemap', () => {
  beforeEach(() => {
    mockFetch.mockClear()
  })

  it('includes static home and explore URLs', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(makeSearchResponse([])))
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const urls = entries.map((e) => e.url)
    // APP_URL defaults to http://localhost:3000 in test env
    expect(urls).toContain('http://localhost:3000')
    expect(urls).toContain('http://localhost:3000/explore')
  })

  it('includes a recipe URL for each public recipe returned', async () => {
    mockFetch.mockResolvedValueOnce(
      makeResponse(
        makeSearchResponse([
          { id: 'r1', createdAt: '2024-01-01T00:00:00Z' },
          { id: 'r2' },
        ])
      )
    )
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const urls = entries.map((e) => e.url)
    expect(urls).toContain('http://localhost:3000/recipes/r1')
    expect(urls).toContain('http://localhost:3000/recipes/r2')
  })

  it('sets lastModified on recipe entries that have createdAt', async () => {
    mockFetch.mockResolvedValueOnce(
      makeResponse(makeSearchResponse([{ id: 'r1', createdAt: '2024-01-01T00:00:00Z' }]))
    )
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const recipeEntry = entries.find((e) => e.url.includes('/recipes/r1'))
    expect(recipeEntry?.lastModified).toBeInstanceOf(Date)
  })

  it('omits lastModified when recipe has no createdAt', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(makeSearchResponse([{ id: 'r2' }])))
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const recipeEntry = entries.find((e) => e.url.includes('/recipes/r2'))
    expect(recipeEntry?.lastModified).toBeUndefined()
  })

  it('paginates through multiple pages of recipes', async () => {
    // First page returns nextCursor
    mockFetch.mockResolvedValueOnce(
      makeResponse(makeSearchResponse([{ id: 'r1' }], 'cursor-abc'))
    )
    // Second page returns no nextCursor
    mockFetch.mockResolvedValueOnce(makeResponse(makeSearchResponse([{ id: 'r2' }])))

    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const urls = entries.map((e) => e.url)
    expect(urls).toContain('http://localhost:3000/recipes/r1')
    expect(urls).toContain('http://localhost:3000/recipes/r2')
    expect(mockFetch).toHaveBeenCalledTimes(2)
  })

  it('handles API errors gracefully and returns static URLs only', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse({ error: 'Service unavailable' }, 503))
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const urls = entries.map((e) => e.url)
    expect(urls).toContain('http://localhost:3000')
    expect(urls).toContain('http://localhost:3000/explore')
    // No recipe URLs since API failed
    expect(urls.some((u) => u.includes('/recipes/'))).toBe(false)
  })

  it('handles fetch exceptions gracefully', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'))
    const { default: sitemap } = await import('@/app/sitemap')
    const entries = await sitemap()
    const urls = entries.map((e) => e.url)
    expect(urls).toContain('http://localhost:3000')
    expect(urls).toContain('http://localhost:3000/explore')
  })
})
