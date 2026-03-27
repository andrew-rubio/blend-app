import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getAdminDashboardCountsApi,
  getFeaturedRecipesApi,
  createFeaturedRecipeApi,
  updateFeaturedRecipeApi,
  deleteFeaturedRecipeApi,
  getStoriesApi,
  createStoryApi,
  updateStoryApi,
  deleteStoryApi,
  getVideosApi,
  createVideoApi,
  updateVideoApi,
  deleteVideoApi,
  getAdminSubmissionsApi,
  approveSubmissionApi,
  rejectSubmissionApi,
  batchApproveSubmissionsApi,
  batchRejectSubmissionsApi,
} from '@/lib/api/admin'
import type {
  CreateFeaturedRecipeRequest,
  UpdateFeaturedRecipeRequest,
  CreateStoryRequest,
  UpdateStoryRequest,
  CreateVideoRequest,
  UpdateVideoRequest,
  ApproveSubmissionRequest,
  RejectSubmissionRequest,
  BatchActionRequest,
  IngredientSubmissionStatus,
} from '@/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const adminQueryKeys = {
  all: ['admin'] as const,
  dashboard: () => [...adminQueryKeys.all, 'dashboard'] as const,
  featuredRecipes: () => [...adminQueryKeys.all, 'featured-recipes'] as const,
  stories: () => [...adminQueryKeys.all, 'stories'] as const,
  videos: () => [...adminQueryKeys.all, 'videos'] as const,
  submissions: (status?: IngredientSubmissionStatus, page?: number) =>
    [...adminQueryKeys.all, 'submissions', status, page] as const,
}

// ── Dashboard ──────────────────────────────────────────────────────────────────

/** Fetches overview counts for the admin dashboard. */
export function useAdminDashboardCounts() {
  return useQuery({
    queryKey: adminQueryKeys.dashboard(),
    queryFn: getAdminDashboardCountsApi,
  })
}

// ── Featured Recipes ──────────────────────────────────────────────────────────

/** Fetches all featured recipes. */
export function useAdminFeaturedRecipes() {
  return useQuery({
    queryKey: adminQueryKeys.featuredRecipes(),
    queryFn: getFeaturedRecipesApi,
  })
}

/** Creates a new featured recipe and invalidates the list. */
export function useCreateFeaturedRecipe() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateFeaturedRecipeRequest) => createFeaturedRecipeApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.featuredRecipes() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

/** Updates a featured recipe and invalidates the list. */
export function useUpdateFeaturedRecipe() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateFeaturedRecipeRequest }) =>
      updateFeaturedRecipeApi(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.featuredRecipes() })
    },
  })
}

/** Deletes a featured recipe and invalidates the list. */
export function useDeleteFeaturedRecipe() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteFeaturedRecipeApi(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.featuredRecipes() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

// ── Stories ────────────────────────────────────────────────────────────────────

/** Fetches all stories. */
export function useAdminStories() {
  return useQuery({
    queryKey: adminQueryKeys.stories(),
    queryFn: getStoriesApi,
  })
}

/** Creates a new story and invalidates the list. */
export function useCreateStory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateStoryRequest) => createStoryApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.stories() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

/** Updates a story and invalidates the list. */
export function useUpdateStory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStoryRequest }) =>
      updateStoryApi(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.stories() })
    },
  })
}

/** Deletes a story and invalidates the list. */
export function useDeleteStory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteStoryApi(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.stories() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

// ── Videos ────────────────────────────────────────────────────────────────────

/** Fetches all videos. */
export function useAdminVideos() {
  return useQuery({
    queryKey: adminQueryKeys.videos(),
    queryFn: getVideosApi,
  })
}

/** Creates a new video and invalidates the list. */
export function useCreateVideo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateVideoRequest) => createVideoApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.videos() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

/** Updates a video and invalidates the list. */
export function useUpdateVideo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateVideoRequest }) =>
      updateVideoApi(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.videos() })
    },
  })
}

/** Deletes a video and invalidates the list. */
export function useDeleteVideo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteVideoApi(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.videos() })
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() })
    },
  })
}

// ── Ingredient Submissions ────────────────────────────────────────────────────

/** Fetches ingredient submissions with optional status filter and pagination. */
export function useAdminSubmissions(status?: IngredientSubmissionStatus, page = 1) {
  return useQuery({
    queryKey: adminQueryKeys.submissions(status, page),
    queryFn: () => getAdminSubmissionsApi(status, page),
  })
}

/** Approves a single ingredient submission. */
export function useApproveSubmission() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data?: ApproveSubmissionRequest }) =>
      approveSubmissionApi(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.all })
    },
  })
}

/** Rejects a single ingredient submission. */
export function useRejectSubmission() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data?: RejectSubmissionRequest }) =>
      rejectSubmissionApi(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.all })
    },
  })
}

/** Batch approves ingredient submissions. */
export function useBatchApproveSubmissions() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: BatchActionRequest) => batchApproveSubmissionsApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.all })
    },
  })
}

/** Batch rejects ingredient submissions. */
export function useBatchRejectSubmissions() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: BatchActionRequest) => batchRejectSubmissionsApi(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminQueryKeys.all })
    },
  })
}
