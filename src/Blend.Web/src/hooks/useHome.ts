import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getHomeApi } from '@/lib/api/home'
import type { HomeResponse } from '@/types'

const HOME_STALE_TIME = 60_000

export const homeQueryKeys = {
  all: ['home'] as const,
  data: () => [...homeQueryKeys.all, 'data'] as const,
}

/**
 * Fetches all home page sections from GET /api/v1/home (HOME-01 through HOME-24).
 */
export function useHome() {
  return useQuery<HomeResponse, { message: string; status: number }>({
    queryKey: homeQueryKeys.data(),
    queryFn: getHomeApi,
    staleTime: HOME_STALE_TIME,
  })
}

/**
 * Returns a function that invalidates and refetches home page data (for pull-to-refresh).
 */
export function useRefreshHome() {
  const queryClient = useQueryClient()
  return async () => {
    await queryClient.invalidateQueries({ queryKey: homeQueryKeys.all })
    await queryClient.refetchQueries({ queryKey: homeQueryKeys.data() })
  }
}
