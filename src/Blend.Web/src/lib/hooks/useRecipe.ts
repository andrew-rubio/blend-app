import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { Recipe } from '@/types/recipe'

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? ''

interface ApiError extends Error {
  status: number
}

function recipeErrorMessage(status: number): string {
  if (status === 404) return 'Recipe not found'
  if (status === 403) return 'This recipe is private'
  return 'Failed to fetch recipe'
}

async function fetchRecipe(id: string): Promise<Recipe> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${id}`)
  if (!res.ok) {
    const err = new Error(recipeErrorMessage(res.status)) as ApiError
    err.status = res.status
    throw err
  }
  return res.json() as Promise<Recipe>
}

async function recordView(id: string): Promise<void> {
  await fetch(`${API_BASE}/api/v1/recipes/${id}/view`, { method: 'POST' })
}

async function toggleLike(id: string, liked: boolean): Promise<void> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${id}/like`, {
    method: liked ? 'DELETE' : 'POST',
  })
  if (!res.ok) {
    const err = new Error('Failed to update like') as ApiError
    err.status = res.status
    throw err
  }
}

export function useRecipe(id: string) {
  return useQuery<Recipe, ApiError>({
    queryKey: ['recipe', id],
    queryFn: async () => {
      const recipe = await fetchRecipe(id)
      void recordView(id)
      return recipe
    },
  })
}

export function useToggleLike(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ liked }: { liked: boolean }) => toggleLike(id, liked),
    onMutate: async ({ liked }) => {
      await queryClient.cancelQueries({ queryKey: ['recipe', id] })
      const prev = queryClient.getQueryData<Recipe>(['recipe', id])
      queryClient.setQueryData<Recipe>(['recipe', id], (old) =>
        old
          ? { ...old, isLiked: !liked, likes: liked ? old.likes - 1 : old.likes + 1 }
          : old,
      )
      return { prev }
    },
    onError: (_err, _vars, context) => {
      if (context?.prev) {
        queryClient.setQueryData(['recipe', id], context.prev)
      }
    },
  })
}
