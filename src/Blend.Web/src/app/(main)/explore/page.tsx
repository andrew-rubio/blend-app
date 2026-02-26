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
  const { query, filters, activeFilterCount, setQuery, setFilters, clearFilters, clearSearch } =
    useSearchStore();
  const [inputValue, setInputValue] = useState(query);
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  const debouncedQuery = useDebounce(inputValue, 300);

  // Sync from URL params on mount
  useEffect(() => {
    const urlQuery = searchParams.get('q') ?? '';
    const urlCuisines = searchParams.get('cuisines')?.split(',').filter(Boolean) ?? [];
    const urlDiets = searchParams.get('diets')?.split(',').filter(Boolean) ?? [];
    const urlDishTypes = searchParams.get('dishTypes')?.split(',').filter(Boolean) ?? [];
    const urlMaxReadyTime = searchParams.get('maxReadyTime');

    if (urlQuery) setInputValue(urlQuery);
    setQuery(urlQuery);
    setFilters({
      cuisines: urlCuisines,
      diets: urlDiets,
      dishTypes: urlDishTypes,
      maxReadyTime: urlMaxReadyTime ? Number(urlMaxReadyTime) : undefined,
    });
  // setInputValue, setQuery, setFilters are stable; searchParams intentionally read once on mount
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Sync debounced query to store and URL
  useEffect(() => {
    setQuery(debouncedQuery);
    const params = new URLSearchParams();
    if (debouncedQuery) params.set('q', debouncedQuery);
    if (filters.cuisines.length) params.set('cuisines', filters.cuisines.join(','));
    if (filters.diets.length) params.set('diets', filters.diets.join(','));
    if (filters.dishTypes.length) params.set('dishTypes', filters.dishTypes.join(','));
    if (filters.maxReadyTime !== undefined) params.set('maxReadyTime', String(filters.maxReadyTime));
    const search = params.toString();
    router.replace(`/explore${search ? `?${search}` : ''}`, { scroll: false });
  // router and setQuery are stable references; listing them would cause unnecessary re-runs
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedQuery, filters]);

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
