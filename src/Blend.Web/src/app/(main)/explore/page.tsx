'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
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

  // Initialise input directly from URL so no state mutation is needed inside an effect
  const [inputValue, setInputValue] = useState(() => searchParams.get('q') ?? '');
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  // Stable refs for values that change identity in tests but are stable in production
  const routerRef = useRef(router);
  routerRef.current = router;
  const setQueryRef = useRef(setQuery);
  setQueryRef.current = setQuery;

  // Capture the initial searchParams snapshot for the mount effect
  const initialSearchParams = useRef(searchParams);

  const debouncedQuery = useDebounce(inputValue, 300);

  // Sync store from URL on mount only (intentional empty deps — reads snapshot once)
  useEffect(() => {
    const sp = initialSearchParams.current;
    const urlQuery = sp.get('q') ?? '';
    const urlCuisines = sp.get('cuisines')?.split(',').filter(Boolean) ?? [];
    const urlDiets = sp.get('diets')?.split(',').filter(Boolean) ?? [];
    const urlDishTypes = sp.get('dishTypes')?.split(',').filter(Boolean) ?? [];
    const urlMaxReadyTime = sp.get('maxReadyTime');

    setQueryRef.current(urlQuery);
    setFilters({
      cuisines: urlCuisines,
      diets: urlDiets,
      dishTypes: urlDishTypes,
      maxReadyTime: urlMaxReadyTime ? Number(urlMaxReadyTime) : undefined,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Sync debounced query + filters to store and URL
  useEffect(() => {
    setQueryRef.current(debouncedQuery);
    const params = new URLSearchParams();
    if (debouncedQuery) params.set('q', debouncedQuery);
    if (filters.cuisines.length) params.set('cuisines', filters.cuisines.join(','));
    if (filters.diets.length) params.set('diets', filters.diets.join(','));
    if (filters.dishTypes.length) params.set('dishTypes', filters.dishTypes.join(','));
    if (filters.maxReadyTime !== undefined) params.set('maxReadyTime', String(filters.maxReadyTime));
    const search = params.toString();
    routerRef.current.replace(`/explore${search ? `?${search}` : ''}`, { scroll: false });
  }, [debouncedQuery, filters]);

  const { data, isLoading, isFetchingNextPage, hasNextPage, error, fetchNextPage, refetch } =
    useSearchRecipes({ query: debouncedQuery, filters });

  const allResults = data?.pages.flatMap((p) => p.results) ?? [];
  const totalResults = data?.pages[0]?.totalResults ?? 0;

  const handleClear = useCallback(() => {
    setInputValue('');
    clearSearch();
    routerRef.current.replace('/explore', { scroll: false });
  }, [clearSearch]);

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
