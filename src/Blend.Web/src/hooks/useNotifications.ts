import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getNotificationsApi,
  getUnreadCountApi,
  markNotificationReadApi,
  markAllNotificationsReadApi,
} from '@/lib/api/notifications'
import { useNotificationStore } from '@/stores/notificationStore'
import type { NotificationsPageResponse } from '@/types'

export const notificationQueryKeys = {
  all: ['notifications'] as const,
  list: () => [...notificationQueryKeys.all, 'list'] as const,
  unreadCount: () => [...notificationQueryKeys.all, 'unread-count'] as const,
}

export function useNotifications() {
  return useInfiniteQuery<NotificationsPageResponse, { message: string; status: number }>({
    queryKey: notificationQueryKeys.list(),
    queryFn: ({ pageParam }) => getNotificationsApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.nextCursor : undefined),
    staleTime: 30_000,
  })
}

export function usePollUnreadCount() {
  const setUnreadCount = useNotificationStore((s) => s.setUnreadCount)
  return async () => {
    try {
      const { count } = await getUnreadCountApi()
      setUnreadCount(count)
    } catch {
      // silently ignore polling errors
    }
  }
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => markNotificationReadApi(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.list() })
      void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.unreadCount() })
    },
  })
}

export function useMarkAllNotificationsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => markAllNotificationsReadApi(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.list() })
      void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.unreadCount() })
    },
  })
}
