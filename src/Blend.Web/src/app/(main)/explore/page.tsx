'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useSearchStore } from '@/stores/searchStore';
import { useSearchRecipes } from '@/hooks/useSearch';
import { useDebounce } from '@/hooks/useDebounce';
import { SearchInput } from '@/components/features/explore/SearchInput';
import { ExploreView } from '@/components/features/explore/ExploreView';
import { SearchResultsView } from '@/components/features/explore/SearchResultsView';
import { FilterPanel } from '@/components/features/explore/FilterPanel';
import { SearchFilters } from '@/types/search';

export default function ExplorePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { filters, activeFilterCount, setQuery, setFilters, clearFilters, clearSearch } =
    useSearchStore();

  // Capture initial URL params once via lazy useState initializer (stable across renders)
  const [initParams] = useState(() => ({
    q: searchParams.get('q') ?? '',
    cuisines: searchParams.get('cuisines')?.split(',').filter(Boolean) ?? [],
    diets: searchParams.get('diets')?.split(',').filter(Boolean) ?? [],
    dishTypes: searchParams.get('dishTypes')?.split(',').filter(Boolean) ?? [],
    maxReadyTime: searchParams.get('maxReadyTime')
      ? Number(searchParams.get('maxReadyTime'))
      : undefined,
  }));

  const [inputValue, setInputValue] = useState(initParams.q);
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  const debouncedQuery = useDebounce(inputValue, 300);

  // Sync store from URL on mount only — initParams is stable so deps are satisfied
  useEffect(() => {
    setQuery(initParams.q);
    setFilters({
      cuisines: initParams.cuisines,
      diets: initParams.diets,
      dishTypes: initParams.dishTypes,
      maxReadyTime: initParams.maxReadyTime,
    });
  }, [initParams, setQuery, setFilters]);

  // Sync debounced query to Zustand store
  useEffect(() => {
    setQuery(debouncedQuery);
  }, [debouncedQuery, setQuery]);

  // Sync debounced query + filters to URL
  useEffect(() => {
    const params = new URLSearchParams();
    if (debouncedQuery) params.set('q', debouncedQuery);
    if (filters.cuisines.length) params.set('cuisines', filters.cuisines.join(','));
    if (filters.diets.length) params.set('diets', filters.diets.join(','));
    if (filters.dishTypes.length) params.set('dishTypes', filters.dishTypes.join(','));
    if (filters.maxReadyTime !== undefined) params.set('maxReadyTime', String(filters.maxReadyTime));
    const search = params.toString();
    router.replace(`/explore${search ? `?${search}` : ''}`, { scroll: false });
  }, [debouncedQuery, filters, router]);

  const { data, isLoading, isFetchingNextPage, hasNextPage, error, fetchNextPage, refetch } =
    useSearchRecipes({ query: debouncedQuery, filters });

  const allResults = data?.pages.flatMap((p) => p.results) ?? [];
  const totalResults = data?.pages[0]?.totalResults ?? 0;

  const handleClear = useCallback(() => {
    setInputValue('');
    clearSearch();
    router.replace('/explore', { scroll: false });
  }, [clearSearch, router]);

  const handleFiltersChange = useCallback(
    (newFilters: SearchFilters) => {
      setFilters(newFilters);
    },
    [setFilters],
  );

  const isSearchActive = debouncedQuery.trim().length > 0;

  return (
    <main className="min-h-screen bg-gray-50">
      <div className="max-w-2xl mx-auto px-4 py-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">Explore</h1>
        <div className="mb-6">
          <SearchInput
            value={inputValue}
            onChange={setInputValue}
            onClear={handleClear}
            activeFilterCount={activeFilterCount}
            onFilterClick={() => setIsFilterOpen(true)}
          />
        </div>
        {isSearchActive ? (
          <SearchResultsView
            query={debouncedQuery}
            results={allResults}
            totalResults={totalResults}
            isLoading={isLoading}
            isFetchingNextPage={isFetchingNextPage}
            hasNextPage={hasNextPage}
            error={error}
            onLoadMore={fetchNextPage}
            onRetry={refetch}
          />
        ) : (
          <ExploreView
            filters={filters}
            onFiltersChange={handleFiltersChange}
            onSearch={setInputValue}
          />
        )}
      </div>
      <FilterPanel
        isOpen={isFilterOpen}
        onClose={() => setIsFilterOpen(false)}
        filters={filters}
        onFiltersChange={handleFiltersChange}
        onClearAll={clearFilters}
      />
    </main>
  );
}
