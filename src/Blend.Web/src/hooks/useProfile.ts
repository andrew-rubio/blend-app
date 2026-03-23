import { useQuery, useMutation, useInfiniteQuery, useQueryClient } from '@tanstack/react-query'
import {
  getMyProfileApi,
  updateMyProfileApi,
  getPublicProfileApi,
  getMyRecipesApi,
  getLikedRecipesApi,
  getPublicUserRecipesApi,
  toggleRecipeVisibilityApi,
  deleteRecipeApi,
  getUploadUrlApi,
  uploadFileToSasUrl,
  completeUploadApi,
} from '@/lib/api/profile'
import type { UpdateProfileRequest, ProfileRecipesResponse } from '@/types'

const PROFILE_STALE_TIME = 2 * 60_000

export const profileQueryKeys = {
  all: ['profile'] as const,
  mine: () => [...profileQueryKeys.all, 'mine'] as const,
  public: (userId: string) => [...profileQueryKeys.all, 'public', userId] as const,
  myRecipes: () => [...profileQueryKeys.all, 'my-recipes'] as const,
  likedRecipes: () => [...profileQueryKeys.all, 'liked-recipes'] as const,
  publicRecipes: (userId: string) => [...profileQueryKeys.all, 'public-recipes', userId] as const,
}

export function useMyProfile() {
  return useQuery({
    queryKey: profileQueryKeys.mine(),
    queryFn: getMyProfileApi,
    staleTime: PROFILE_STALE_TIME,
  })
}

export function usePublicProfile(userId: string) {
  return useQuery({
    queryKey: profileQueryKeys.public(userId),
    queryFn: () => getPublicProfileApi(userId),
    staleTime: PROFILE_STALE_TIME,
    enabled: !!userId,
  })
}

export function useUpdateProfile() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: UpdateProfileRequest) => updateMyProfileApi(data),
    onSuccess: (updated) => {
      queryClient.setQueryData(profileQueryKeys.mine(), updated)
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: profileQueryKeys.mine() })
    },
  })
}

export function useMyRecipes() {
  return useInfiniteQuery<ProfileRecipesResponse, { message: string; status: number }>({
    queryKey: profileQueryKeys.myRecipes(),
    queryFn: ({ pageParam }) => getMyRecipesApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: PROFILE_STALE_TIME,
  })
}

export function useLikedRecipes() {
  return useInfiniteQuery<ProfileRecipesResponse, { message: string; status: number }>({
    queryKey: profileQueryKeys.likedRecipes(),
    queryFn: ({ pageParam }) => getLikedRecipesApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: PROFILE_STALE_TIME,
  })
}

export function usePublicUserRecipes(userId: string) {
  return useInfiniteQuery<ProfileRecipesResponse, { message: string; status: number }>({
    queryKey: profileQueryKeys.publicRecipes(userId),
    queryFn: ({ pageParam }) => getPublicUserRecipesApi(userId, pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: PROFILE_STALE_TIME,
    enabled: !!userId,
  })
}

export function useToggleRecipeVisibility() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ recipeId, isPublic }: { recipeId: string; isPublic: boolean }) =>
      toggleRecipeVisibilityApi(recipeId, isPublic),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: profileQueryKeys.myRecipes() })
    },
  })
}

export function useDeleteRecipe() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (recipeId: string) => deleteRecipeApi(recipeId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: profileQueryKeys.myRecipes() })
      void queryClient.invalidateQueries({ queryKey: profileQueryKeys.mine() })
    },
  })
}

export function useAvatarUpload() {
  return useMutation({
    mutationFn: async (file: File): Promise<string> => {
      const { sasUrl, blobPath } = await getUploadUrlApi({
        contentType: file.type,
        fileSizeBytes: file.size,
        uploadUse: 'Profile',
      })
      await uploadFileToSasUrl(sasUrl, file)
      const { mediaUrl } = await completeUploadApi({ blobPath, uploadUse: 'Profile' })
      return mediaUrl
    },
  })
}
