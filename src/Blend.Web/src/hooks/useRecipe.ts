import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import {
  getRecipeApi,
  recordViewApi,
  likeRecipeApi,
  unlikeRecipeApi,
  updateRecipeApi,
  type UpdateRecipePayload,
} from '@/lib/api/recipes'
import type { Recipe } from '@/types'

const RECIPE_STALE_TIME = 5 * 60_000

// ── Query keys ─────────────────────────────────────────────────────────────────

export const recipeQueryKeys = {
  all: ['recipes'] as const,
  detail: (id: string) => [...recipeQueryKeys.all, 'detail', id] as const,
}

// ── Fetch recipe detail ────────────────────────────────────────────────────────

/**
 * Fetches the full recipe detail from GET /api/v1/recipes/{id}.
 * Also records a view event once the data is successfully loaded (EXPL-34).
 */
export function useRecipe(id: string) {
  const query = useQuery<Recipe, { message: string; status: number }>({
    queryKey: recipeQueryKeys.detail(id),
    queryFn: () => getRecipeApi(id),
    staleTime: RECIPE_STALE_TIME,
    retry: (failureCount, error) => {
      // Do not retry on 404 or 403
      if (error.status === 404 || error.status === 403) return false
      return failureCount < 2
    },
  })

  // Record view once recipe is loaded (fire-and-forget)
  useEffect(() => {
    if (query.isSuccess && query.data) {
      recordViewApi(id).catch(() => {
        // Silently ignore view tracking failures
      })
    }
  }, [id, query.isSuccess, query.data])

  return query
}

// ── Like / Unlike recipe ───────────────────────────────────────────────────────

/**
 * Toggles the like state for a recipe with optimistic UI update (EXPL-30, EXPL-31).
 * Returns a mutation that accepts `{ id, isCurrentlyLiked }` and optimistically
 * flips the `isLiked` flag and adjusts `likeCount` before the request settles.
 */
export function useLikeRecipe() {
  const queryClient = useQueryClient()

  return useMutation<void, { message: string; status: number }, { id: string; isCurrentlyLiked: boolean }>({
    mutationFn: ({ id, isCurrentlyLiked }) =>
      isCurrentlyLiked ? unlikeRecipeApi(id) : likeRecipeApi(id),

    onMutate: async ({ id, isCurrentlyLiked }) => {
      const queryKey = recipeQueryKeys.detail(id)
      // Cancel any outgoing refetches so they don't overwrite optimistic update
      await queryClient.cancelQueries({ queryKey })

      // Snapshot previous value for rollback
      const previous = queryClient.getQueryData<Recipe>(queryKey)

      // Optimistically update
      queryClient.setQueryData<Recipe>(queryKey, (old) => {
        if (!old) return old
        return {
          ...old,
          isLiked: !isCurrentlyLiked,
          likeCount: isCurrentlyLiked ? old.likeCount - 1 : old.likeCount + 1,
        }
      })

      return { previous }
    },

    onError: (_err, { id }, context) => {
      // Roll back to previous value on error
      const ctx = context as { previous?: Recipe } | undefined
      if (ctx?.previous) {
        queryClient.setQueryData(recipeQueryKeys.detail(id), ctx.previous)
      }
    },

    onSettled: (_data, _err, { id }) => {
      queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(id) })
    },
  })
}

// ── Update recipe ─────────────────────────────────────────────────────────────

/**
 * Updates a community recipe (REQ-60).
 * Calls PUT /api/v1/recipes/{id} and refreshes the cached recipe.
 */
export function useUpdateRecipe(id: string) {
  const queryClient = useQueryClient()
  return useMutation<Recipe, { message: string; status: number }, UpdateRecipePayload>({
    mutationFn: (payload) => updateRecipeApi(id, payload),
    onSuccess: (updated) => {
      queryClient.setQueryData(recipeQueryKeys.detail(id), updated)
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(id) })
    },
  })
}
