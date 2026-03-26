'use client'

import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useNotifications, useMarkNotificationRead, useMarkAllNotificationsRead, notificationQueryKeys } from '@/hooks/useNotifications'
import { NotificationItem } from './NotificationItem'

export function NotificationList() {
  const queryClient = useQueryClient()

  useEffect(() => {
    void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.list() })
  }, [queryClient])

  const {
    data,
    isLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useNotifications()

  const { mutate: markRead, isPending: isMarkingRead } = useMarkNotificationRead()
  const { mutate: markAllRead, isPending: isMarkingAll } = useMarkAllNotificationsRead()

  const notifications = data?.pages.flatMap((p) => p.items) ?? []
  const hasUnread = notifications.some((n) => !n.isRead)

  return (
    <div className="mx-auto max-w-xl px-4 py-8">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Notifications</h1>
        {hasUnread && (
          <button
            onClick={() => markAllRead()}
            disabled={isMarkingAll}
            className="text-sm font-medium text-primary-600 hover:text-primary-700 disabled:opacity-50"
            aria-label="Mark all notifications as read"
          >
            Mark all as read
          </button>
        )}
      </div>

      {isLoading ? (
        <p className="text-center text-gray-500" aria-label="Loading notifications">Loading…</p>
      ) : notifications.length === 0 ? (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center dark:border-gray-700">
          <p className="text-gray-500">You have no notifications yet.</p>
        </div>
      ) : (
        <ul className="divide-y divide-gray-200 dark:divide-gray-800" aria-label="Notifications list">
          {notifications.map((n) => (
            <li key={n.id}>
              <NotificationItem
                notification={n}
                onMarkRead={(id) => markRead(id)}
                isMarkingRead={isMarkingRead}
              />
            </li>
          ))}
        </ul>
      )}

      {hasNextPage && (
        <button
          onClick={() => void fetchNextPage()}
          disabled={isFetchingNextPage}
          className="mt-4 w-full rounded-md border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50"
          aria-label="Load more notifications"
        >
          {isFetchingNextPage ? 'Loading…' : 'Load more'}
        </button>
      )}
    </div>
  )
}
