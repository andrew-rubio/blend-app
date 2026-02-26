'use client';

import { useInfiniteQuery } from '@tanstack/react-query';
import { searchRecipes } from '@/lib/api/search';
import { SearchFilters, SearchResponse } from '@/types/search';

interface UseSearchRecipesParams {
  query: string;
  filters: SearchFilters;
  enabled?: boolean;
}

export function useSearchRecipes({ query, filters, enabled = true }: UseSearchRecipesParams) {
  return useInfiniteQuery<SearchResponse, Error, { pages: SearchResponse[] }, [string, string, SearchFilters], string | undefined>({
    queryKey: ['search', query, filters],
    queryFn: ({ pageParam }) =>
      searchRecipes({ q: query, filters, cursor: pageParam, pageSize: 20 }),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    enabled: enabled && query.trim().length > 0,
  });
}
