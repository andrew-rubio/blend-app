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
  return useInfiniteQuery({
    queryKey: ['search', query, filters] as const,
    queryFn: ({ pageParam }: { pageParam: string | undefined }) =>
      searchRecipes({ q: query, filters, cursor: pageParam, pageSize: 20 }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage: SearchResponse) => lastPage.nextCursor ?? undefined,
    enabled: enabled && query.trim().length > 0,
  });
}
