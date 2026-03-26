import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  createIngredientSubmissionApi,
  getMyIngredientSubmissionsApi,
  searchIngredientsApi,
  getIngredientCatalogueApi,
} from '@/lib/api/ingredientSubmissions'
import type { CreateIngredientSubmissionRequest } from '@/types'

export const ingredientSubmissionQueryKeys = {
  all: ['ingredient-submissions'] as const,
  mine: () => [...ingredientSubmissionQueryKeys.all, 'mine'] as const,
  catalogue: (cursor?: string) => [...ingredientSubmissionQueryKeys.all, 'catalogue', cursor] as const,
  search: (q: string) => [...ingredientSubmissionQueryKeys.all, 'search', q] as const,
}

/**
 * Fetches all ingredient submissions by the current user.
 */
export function useMyIngredientSubmissions() {
  return useQuery({
    queryKey: ingredientSubmissionQueryKeys.mine(),
    queryFn: getMyIngredientSubmissionsApi,
  })
}

/**
 * Submits a new ingredient to the knowledge base.
 */
export function useCreateIngredientSubmission() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateIngredientSubmissionRequest) => createIngredientSubmissionApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ingredientSubmissionQueryKeys.mine() })
    },
  })
}

/**
 * Searches the ingredient knowledge base catalogue.
 */
export function useIngredientSearch(query: string) {
  return useQuery({
    queryKey: ingredientSubmissionQueryKeys.search(query),
    queryFn: () => searchIngredientsApi(query),
    enabled: query.trim().length > 0,
    staleTime: 30_000,
  })
}

/**
 * Fetches a page from the ingredient catalogue.
 */
export function useIngredientCatalogue(cursor?: string) {
  return useQuery({
    queryKey: ingredientSubmissionQueryKeys.catalogue(cursor),
    queryFn: () => getIngredientCatalogueApi(cursor),
    staleTime: 60_000,
  })
}
