import type { MetadataRoute } from 'next'

const APP_URL = (process.env.NEXT_PUBLIC_APP_URL ?? 'http://localhost:3000').replace(/\/$/, '')
const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000').replace(/\/$/, '')

interface RecipeSummary {
  id: string
  createdAt?: string
}

interface SearchApiResponse {
  results: RecipeSummary[]
  metadata: {
    nextCursor?: string
  }
}

/** Fetch all public recipe IDs by paging through the search endpoint. */
async function fetchAllPublicRecipes(): Promise<RecipeSummary[]> {
  const recipes: RecipeSummary[] = []
  let cursor: string | undefined

  do {
    try {
      const url = new URL(`${API_URL}/api/v1/search/recipes`)
      url.searchParams.set('pageSize', '100')
      if (cursor) {
        url.searchParams.set('cursor', cursor)
      }

      const response = await fetch(url.toString(), { next: { revalidate: 3600 } })
      if (!response.ok) break

      const data = (await response.json()) as SearchApiResponse
      recipes.push(...data.results)
      cursor = data.metadata.nextCursor
    } catch {
      break
    }
  } while (cursor)

  return recipes
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticUrls: MetadataRoute.Sitemap = [
    {
      url: APP_URL,
      changeFrequency: 'daily',
      priority: 1.0,
    },
    {
      url: `${APP_URL}/explore`,
      changeFrequency: 'daily',
      priority: 0.9,
    },
  ]

  const recipes = await fetchAllPublicRecipes()
  const recipeUrls: MetadataRoute.Sitemap = recipes.map((recipe) => ({
    url: `${APP_URL}/recipes/${encodeURIComponent(recipe.id)}`,
    changeFrequency: 'weekly',
    priority: 0.7,
    ...(recipe.createdAt ? { lastModified: new Date(recipe.createdAt) } : {}),
  }))

  return [...staticUrls, ...recipeUrls]
}
