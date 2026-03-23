import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  createSessionApi,
  getActiveSessionApi,
  getSessionApi,
  addIngredientApi,
  removeIngredientApi,
  addDishApi,
  removeDishApi,
  pauseSessionApi,
  completeSessionApi,
  getSuggestionsApi,
  getIngredientDetailApi,
  searchIngredientsApi,
  submitFeedbackApi,
  publishSessionApi,
} from '@/lib/api/cookMode'
import type {
  CookingSession,
  CreateCookSessionRequest,
  AddIngredientRequest,
  AddDishRequest,
  SubmitFeedbackRequest,
  PublishSessionRequest,
  PublishSessionResult,
} from '@/types'

const SESSION_STALE_TIME = 30_000
const SUGGESTIONS_STALE_TIME = 60_000

// ── Query keys ─────────────────────────────────────────────────────────────────

export const cookModeQueryKeys = {
  all: ['cookMode'] as const,
  session: (id: string) => [...cookModeQueryKeys.all, 'session', id] as const,
  activeSession: () => [...cookModeQueryKeys.all, 'active'] as const,
  suggestions: (sessionId: string, dishId?: string) =>
    [...cookModeQueryKeys.all, 'suggestions', sessionId, dishId] as const,
  ingredientDetail: (sessionId: string, ingredientId: string) =>
    [...cookModeQueryKeys.all, 'detail', sessionId, ingredientId] as const,
  ingredientSearch: (q: string) => [...cookModeQueryKeys.all, 'ingredientSearch', q] as const,
}

// ── Queries ────────────────────────────────────────────────────────────────────

export function useActiveSession() {
  return useQuery({
    queryKey: cookModeQueryKeys.activeSession(),
    queryFn: getActiveSessionApi,
    staleTime: SESSION_STALE_TIME,
    retry: false,
  })
}

export function useSession(id: string) {
  return useQuery({
    queryKey: cookModeQueryKeys.session(id),
    queryFn: () => getSessionApi(id),
    staleTime: SESSION_STALE_TIME,
    enabled: Boolean(id),
  })
}

export function useSuggestions(sessionId: string, dishId?: string) {
  return useQuery({
    queryKey: cookModeQueryKeys.suggestions(sessionId, dishId),
    queryFn: () => getSuggestionsApi(sessionId, dishId),
    staleTime: SUGGESTIONS_STALE_TIME,
    enabled: Boolean(sessionId),
  })
}

export function useIngredientDetail(sessionId: string, ingredientId: string) {
  return useQuery({
    queryKey: cookModeQueryKeys.ingredientDetail(sessionId, ingredientId),
    queryFn: () => getIngredientDetailApi(sessionId, ingredientId),
    staleTime: SUGGESTIONS_STALE_TIME,
    enabled: Boolean(sessionId) && Boolean(ingredientId),
  })
}

export function useIngredientSearch(q: string) {
  return useQuery({
    queryKey: cookModeQueryKeys.ingredientSearch(q),
    queryFn: () => searchIngredientsApi(q),
    staleTime: 30_000,
    enabled: q.length >= 2,
  })
}

// ── Mutations ──────────────────────────────────────────────────────────────────

export function useCreateSession() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (req: CreateCookSessionRequest) => createSessionApi(req),
    onSuccess: (session) => {
      queryClient.setQueryData(cookModeQueryKeys.session(session.id), session)
    },
  })
}

export function useAddIngredient(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (req: AddIngredientRequest) => addIngredientApi(sessionId, req),
    onMutate: async (req) => {
      await queryClient.cancelQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
      const snapshot = queryClient.getQueryData<CookingSession>(cookModeQueryKeys.session(sessionId))
      if (snapshot) {
        const newIngredient = {
          ingredientId: req.ingredientId,
          name: req.name,
          addedAt: new Date().toISOString(),
          notes: req.notes,
        }
        const updated: CookingSession = req.dishId
          ? {
              ...snapshot,
              dishes: snapshot.dishes.map((d) =>
                d.dishId === req.dishId
                  ? { ...d, ingredients: [...d.ingredients, newIngredient] }
                  : d
              ),
            }
          : { ...snapshot, addedIngredients: [...snapshot.addedIngredients, newIngredient] }
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), updated)
      }
      return { snapshot }
    },
    onError: (_err, _req, context) => {
      if (context?.snapshot) {
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), context.snapshot)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
    },
  })
}

export function useRemoveIngredient(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ ingredientId, dishId }: { ingredientId: string; dishId?: string }) =>
      removeIngredientApi(sessionId, ingredientId, dishId),
    onMutate: async ({ ingredientId, dishId }) => {
      await queryClient.cancelQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
      const snapshot = queryClient.getQueryData<CookingSession>(cookModeQueryKeys.session(sessionId))
      if (snapshot) {
        const updated: CookingSession = dishId
          ? {
              ...snapshot,
              dishes: snapshot.dishes.map((d) =>
                d.dishId === dishId
                  ? { ...d, ingredients: d.ingredients.filter((i) => i.ingredientId !== ingredientId) }
                  : d
              ),
            }
          : {
              ...snapshot,
              addedIngredients: snapshot.addedIngredients.filter((i) => i.ingredientId !== ingredientId),
            }
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), updated)
      }
      return { snapshot }
    },
    onError: (_err, _req, context) => {
      if (context?.snapshot) {
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), context.snapshot)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
    },
  })
}

export function useAddDish(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (req: AddDishRequest) => addDishApi(sessionId, req),
    onMutate: async (req) => {
      await queryClient.cancelQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
      const snapshot = queryClient.getQueryData<CookingSession>(cookModeQueryKeys.session(sessionId))
      if (snapshot) {
        const tempDish = {
          dishId: `temp-${Date.now()}`,
          name: req.name,
          cuisineType: req.cuisineType,
          ingredients: [],
          notes: req.notes,
        }
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), {
          ...snapshot,
          dishes: [...snapshot.dishes, tempDish],
        })
      }
      return { snapshot }
    },
    onError: (_err, _req, context) => {
      if (context?.snapshot) {
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), context.snapshot)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
    },
  })
}

export function useRemoveDish(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dishId: string) => removeDishApi(sessionId, dishId),
    onMutate: async (dishId) => {
      await queryClient.cancelQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
      const snapshot = queryClient.getQueryData<CookingSession>(cookModeQueryKeys.session(sessionId))
      if (snapshot) {
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), {
          ...snapshot,
          dishes: snapshot.dishes.filter((d) => d.dishId !== dishId),
        })
      }
      return { snapshot }
    },
    onError: (_err, _req, context) => {
      if (context?.snapshot) {
        queryClient.setQueryData(cookModeQueryKeys.session(sessionId), context.snapshot)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: cookModeQueryKeys.session(sessionId) })
    },
  })
}

export function usePauseSession(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => pauseSessionApi(sessionId),
    onSuccess: (session) => {
      queryClient.setQueryData(cookModeQueryKeys.session(sessionId), session)
    },
  })
}

export function useCompleteSession(sessionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => completeSessionApi(sessionId),
    onSuccess: (session) => {
      queryClient.setQueryData(cookModeQueryKeys.session(sessionId), session)
    },
  })
}

export function useSubmitFeedback(sessionId: string) {
  return useMutation({
    mutationFn: (req: SubmitFeedbackRequest) => submitFeedbackApi(sessionId, req),
  })
}

export function usePublishSession(sessionId: string) {
  return useMutation({
    mutationFn: (req: PublishSessionRequest) => publishSessionApi(sessionId, req),
  })
}

// Convenience re-export so callers can import the result type without a separate import
export type { PublishSessionResult }
