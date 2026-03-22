import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getPreferencesApi,
  updatePreferencesApi,
  getCuisinesApi,
  getDishTypesApi,
  getDietsApi,
  getIntolerancesApi,
} from '@/lib/api/preferences'
import type { UpdatePreferencesRequest } from '@/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const preferenceQueryKeys = {
  all: ['preferences'] as const,
  userPreferences: () => [...preferenceQueryKeys.all, 'user'] as const,
  cuisines: () => [...preferenceQueryKeys.all, 'lists', 'cuisines'] as const,
  dishTypes: () => [...preferenceQueryKeys.all, 'lists', 'dish-types'] as const,
  diets: () => [...preferenceQueryKeys.all, 'lists', 'diets'] as const,
  intolerances: () => [...preferenceQueryKeys.all, 'lists', 'intolerances'] as const,
}

// ── User preferences ──────────────────────────────────────────────────────────

/** Fetches the authenticated user's saved preferences. */
export function useUserPreferences() {
  return useQuery({
    queryKey: preferenceQueryKeys.userPreferences(),
    queryFn: getPreferencesApi,
  })
}

/** Saves the user's preferences and invalidates related query caches. */
export function useSavePreferences() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: UpdatePreferencesRequest) => updatePreferencesApi(data),
    onSuccess: () => {
      // Invalidate saved preferences so they reload with new values
      void queryClient.invalidateQueries({ queryKey: preferenceQueryKeys.userPreferences() })
      // Invalidate search and recommendation caches so they reflect the new preferences (PREF-18)
      void queryClient.invalidateQueries({ queryKey: ['search'] })
      void queryClient.invalidateQueries({ queryKey: ['recommendations'] })
    },
  })
}

// ── Reference lists ────────────────────────────────────────────────────────────

/** Fetches the list of supported cuisine types from the backend. */
export function useCuisines() {
  return useQuery({
    queryKey: preferenceQueryKeys.cuisines(),
    queryFn: getCuisinesApi,
    staleTime: Infinity,
  })
}

/** Fetches the list of supported dish types from the backend. */
export function useDishTypes() {
  return useQuery({
    queryKey: preferenceQueryKeys.dishTypes(),
    queryFn: getDishTypesApi,
    staleTime: Infinity,
  })
}

/** Fetches the list of supported dietary plans from the backend. */
export function useDiets() {
  return useQuery({
    queryKey: preferenceQueryKeys.diets(),
    queryFn: getDietsApi,
    staleTime: Infinity,
  })
}

/** Fetches the list of supported intolerances from the backend. */
export function useIntolerances() {
  return useQuery({
    queryKey: preferenceQueryKeys.intolerances(),
    queryFn: getIntolerancesApi,
    staleTime: Infinity,
  })
}
