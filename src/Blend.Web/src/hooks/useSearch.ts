import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { searchRecipesApi } from '@/lib/api/search'
import type { SearchFilters, UnifiedSearchResponse } from '@/types'

const SEARCH_STALE_TIME = 60_000
const TRENDING_STALE_TIME = 5 * 60_000

// ── Query keys ─────────────────────────────────────────────────────────────────

export const searchQueryKeys = {
  all: ['search'] as const,
  recipes: (q: string, filters: SearchFilters, sort: string) =>
    [...searchQueryKeys.all, 'recipes', { q, filters, sort }] as const,
  trending: () => [...searchQueryKeys.all, 'trending'] as const,
  recommended: () => [...searchQueryKeys.all, 'recommended'] as const,
}

// ── Search recipes (infinite query for pagination) ─────────────────────────────

export interface UseSearchRecipesOptions {
  query: string
  filters: SearchFilters
  sort?: string
  pageSize?: number
  enabled?: boolean
}

/**
 * Performs a paginated recipe search with infinite scroll support (EXPL-08, EXPL-16).
 */
export function useSearchRecipes({
  query,
  filters,
  sort = 'relevance',
  pageSize = 20,
  enabled = true,
}: UseSearchRecipesOptions) {
  return useInfiniteQuery<
    UnifiedSearchResponse,
    Error,
    { pages: UnifiedSearchResponse[] },
    ReturnType<typeof searchQueryKeys.recipes>,
    string | undefined
  >({
    queryKey: searchQueryKeys.recipes(query, filters, sort),
    queryFn: ({ pageParam }) =>
      searchRecipesApi({
        q: query || undefined,
        cuisines: filters.cuisines.length ? filters.cuisines.join(',') : undefined,
        diets: filters.diets.length ? filters.diets.join(',') : undefined,
        dishTypes: filters.dishTypes.length ? filters.dishTypes.join(',') : undefined,
        maxReadyTime: filters.maxReadyTime ?? undefined,
        sort,
        cursor: pageParam,
        pageSize,
      }),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => lastPage.metadata.nextCursor ?? undefined,
    enabled,
    staleTime: SEARCH_STALE_TIME,
  })
}

// ── Trending recipes ───────────────────────────────────────────────────────────

/**
 * Fetches trending recipes for the Explore landing page (EXPL-02).
 * Uses a popularity sort with an empty query.
 */
export function useTrendingRecipes() {
  return useQuery({
    queryKey: searchQueryKeys.trending(),
    queryFn: () =>
      searchRecipesApi({ sort: 'popularity', pageSize: 20 }).then((r) => r.results),
    staleTime: TRENDING_STALE_TIME,
  })
}

// ── Recommended recipes ────────────────────────────────────────────────────────

/**
 * Fetches personalised recommendations for the Explore landing page (EXPL-04).
 * Uses the search endpoint with the relevance sort (personalised when authenticated).
 */
export function useRecommendedRecipes() {
  return useQuery({
    queryKey: searchQueryKeys.recommended(),
    queryFn: () =>
      searchRecipesApi({ sort: 'relevance', pageSize: 20 }).then((r) => r.results),
    staleTime: TRENDING_STALE_TIME,
  })
}
