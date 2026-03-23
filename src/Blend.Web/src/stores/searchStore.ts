import { create } from 'zustand'
import type { SearchFilters } from '@/types'

// ── Initial state ──────────────────────────────────────────────────────────────

const initialFilters: SearchFilters = {
  cuisines: [],
  diets: [],
  dishTypes: [],
  maxReadyTime: null,
}

// ── State & Actions ────────────────────────────────────────────────────────────

interface SearchState {
  /** The current free-text search query. */
  query: string
  /** Active filter values (EXPL-11). */
  filters: SearchFilters
  /** Controls visibility of the filter panel (EXPL-10). */
  isFilterPanelOpen: boolean
}

interface SearchActions {
  setQuery: (q: string) => void
  setFilters: (filters: Partial<SearchFilters>) => void
  toggleCuisineFilter: (cuisine: string) => void
  toggleDietFilter: (diet: string) => void
  toggleDishTypeFilter: (dishType: string) => void
  setMaxReadyTime: (time: number | null) => void
  clearFilters: () => void
  openFilterPanel: () => void
  closeFilterPanel: () => void
  reset: () => void
}

export type SearchStore = SearchState & SearchActions

const initialState: SearchState = {
  query: '',
  filters: initialFilters,
  isFilterPanelOpen: false,
}

export const useSearchStore = create<SearchStore>()((set) => ({
  ...initialState,

  setQuery: (q: string) => set({ query: q }),

  setFilters: (filters: Partial<SearchFilters>) =>
    set((state) => ({ filters: { ...state.filters, ...filters } })),

  toggleCuisineFilter: (cuisine: string) =>
    set((state) => {
      const current = state.filters.cuisines
      const next = current.includes(cuisine)
        ? current.filter((c) => c !== cuisine)
        : [...current, cuisine]
      return { filters: { ...state.filters, cuisines: next } }
    }),

  toggleDietFilter: (diet: string) =>
    set((state) => {
      const current = state.filters.diets
      const next = current.includes(diet)
        ? current.filter((d) => d !== diet)
        : [...current, diet]
      return { filters: { ...state.filters, diets: next } }
    }),

  toggleDishTypeFilter: (dishType: string) =>
    set((state) => {
      const current = state.filters.dishTypes
      const next = current.includes(dishType)
        ? current.filter((d) => d !== dishType)
        : [...current, dishType]
      return { filters: { ...state.filters, dishTypes: next } }
    }),

  setMaxReadyTime: (time: number | null) =>
    set((state) => ({ filters: { ...state.filters, maxReadyTime: time } })),

  clearFilters: () => set({ filters: initialFilters }),

  openFilterPanel: () => set({ isFilterPanelOpen: true }),

  closeFilterPanel: () => set({ isFilterPanelOpen: false }),

  reset: () => set(initialState),
}))

/** Returns the total number of active filters. */
export function selectActiveFilterCount(filters: SearchFilters): number {
  return (
    filters.cuisines.length +
    filters.diets.length +
    filters.dishTypes.length +
    (filters.maxReadyTime != null ? 1 : 0)
  )
}
