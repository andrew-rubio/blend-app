'use client'

import { useRouter } from 'next/navigation'
import type { ApiNotificationItem, NotificationType } from '@/types'

function formatRelativeTime(createdAt: string): string {
  const diff = Date.now() - new Date(createdAt).getTime()
  const minutes = Math.floor(diff / 60_000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  return `${days}d ago`
}

function getNotificationIcon(type: NotificationType): string {
  switch (type) {
    case 'friendRequestReceived':
      return '👋'
    case 'friendRequestAccepted':
      return '🤝'
    case 'recipeLiked':
      return '❤️'
    case 'recipePublished':
      return '📗'
    default:
      return '🔔'
  }
}

function getNotificationMessage(item: ApiNotificationItem): string {
  switch (item.type) {
    case 'friendRequestReceived':
      return `${item.actorDisplayName ?? 'Someone'} sent you a friend request`
    case 'friendRequestAccepted':
      return `${item.actorDisplayName ?? 'Someone'} accepted your friend request`
    case 'recipeLiked':
      return `${item.actorDisplayName ?? 'Someone'} liked your recipe${item.recipeTitle ? ` "${item.recipeTitle}"` : ''}`
    case 'recipePublished':
      return `Your recipe${item.recipeTitle ? ` "${item.recipeTitle}"` : ''} has been published`
    default:
      return 'New notification'
  }
}

function getNotificationTarget(item: ApiNotificationItem): string {
  switch (item.type) {
    case 'friendRequestReceived':
      return '/friends?tab=requests'
    case 'friendRequestAccepted':
      return item.targetUserId ? `/users/${item.targetUserId}/profile` : '/friends'
    case 'recipeLiked':
    case 'recipePublished':
      return item.recipeId ? `/recipes/${item.recipeId}` : '/'
    default:
      return '/notifications'
  }
}

interface NotificationItemProps {
  notification: ApiNotificationItem
  onMarkRead: (id: string) => void
  isMarkingRead?: boolean
}

export function NotificationItem({ notification, onMarkRead, isMarkingRead }: NotificationItemProps) {
  const router = useRouter()

  const handleClick = () => {
    if (!notification.isRead) {
      onMarkRead(notification.id)
    }
    router.push(getNotificationTarget(notification))
  }

  return (
    <button
      onClick={handleClick}
      disabled={isMarkingRead}
      className={`flex w-full items-start gap-3 rounded-lg p-4 text-left transition-colors hover:bg-gray-50 dark:hover:bg-gray-800 ${
        notification.isRead ? 'opacity-60' : ''
      }`}
      aria-label={`${getNotificationMessage(notification)} — ${formatRelativeTime(notification.createdAt)}${notification.isRead ? '' : ' (unread)'}`}
    >
      <span className="text-2xl shrink-0" aria-hidden="true">
        {getNotificationIcon(notification.type)}
      </span>
      <div className="flex-1 min-w-0">
        <p className="text-sm text-gray-900 dark:text-gray-100">
          {getNotificationMessage(notification)}
          {!notification.isRead && (
            <span
              className="ml-2 inline-block h-2 w-2 rounded-full bg-primary-600 align-middle"
              aria-hidden="true"
            />
          )}
        </p>
        <p className="mt-0.5 text-xs text-gray-500 dark:text-gray-400">
          {formatRelativeTime(notification.createdAt)}
        </p>
      </div>
    </button>
  )
}
