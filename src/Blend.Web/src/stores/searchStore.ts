import { create } from 'zustand';
import { SearchFilters } from '@/types/search';

interface SearchState {
  query: string;
  filters: SearchFilters;
  activeFilterCount: number;
  setQuery: (query: string) => void;
  setFilters: (filters: SearchFilters) => void;
  clearFilters: () => void;
  clearSearch: () => void;
}

const defaultFilters: SearchFilters = {
  cuisines: [],
  diets: [],
  dishTypes: [],
  maxReadyTime: undefined,
};

function computeActiveFilterCount(filters: SearchFilters): number {
  return (
    filters.cuisines.length +
    filters.diets.length +
    filters.dishTypes.length +
    (filters.maxReadyTime !== undefined ? 1 : 0)
  );
}

export const useSearchStore = create<SearchState>((set) => ({
  query: '',
  filters: defaultFilters,
  activeFilterCount: 0,
  setQuery: (query) => set({ query }),
  setFilters: (filters) =>
    set({ filters, activeFilterCount: computeActiveFilterCount(filters) }),
  clearFilters: () =>
    set({ filters: defaultFilters, activeFilterCount: 0 }),
  clearSearch: () =>
    set({ query: '', filters: defaultFilters, activeFilterCount: 0 }),
}));
