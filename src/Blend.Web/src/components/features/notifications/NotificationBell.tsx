'use client'

import { useEffect } from 'react'
import Link from 'next/link'
import { useNotificationStore } from '@/stores/notificationStore'
import { usePollUnreadCount } from '@/hooks/useNotifications'
import { useAdaptivePolling } from '@/hooks/useAdaptivePolling'

export function NotificationBell() {
  const unreadCount = useNotificationStore((s) => s.unreadCount)
  const pollUnreadCount = usePollUnreadCount()

  useAdaptivePolling({ onPoll: pollUnreadCount })

  useEffect(() => {
    void pollUnreadCount()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <Link
      href="/notifications"
      className="relative inline-flex items-center justify-center rounded-md p-2 text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100"
      aria-label={
        unreadCount > 0
          ? `Notifications, ${unreadCount} unread`
          : 'Notifications'
      }
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        className="h-6 w-6"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        strokeWidth={2}
        aria-hidden="true"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
        />
      </svg>

      {unreadCount > 0 && (
        <span
          className="absolute right-0.5 top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white"
          aria-hidden="true"
        >
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </Link>
  )
}
