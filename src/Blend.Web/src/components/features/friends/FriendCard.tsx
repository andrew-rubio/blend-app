'use client'

import Link from 'next/link'
import type { FriendItem } from '@/types'

interface FriendCardProps {
  friend: FriendItem
  onRemove?: (userId: string) => void
  isRemoving?: boolean
}

export function FriendCard({ friend, onRemove, isRemoving }: FriendCardProps) {
  return (
    <div className="flex items-center gap-4 rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-950">
      <div
        className="flex h-12 w-12 items-center justify-center rounded-full bg-primary-100 text-primary-700 font-semibold text-lg shrink-0"
        aria-hidden="true"
      >
        {friend.avatarUrl ? (
          <img src={friend.avatarUrl} alt="" className="h-12 w-12 rounded-full object-cover" />
        ) : (
          friend.displayName.charAt(0).toUpperCase()
        )}
      </div>
      <div className="flex-1 min-w-0">
        <p className="font-semibold text-gray-900 dark:text-gray-100 truncate">{friend.displayName}</p>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {friend.recipeCount} {friend.recipeCount === 1 ? 'recipe' : 'recipes'}
        </p>
      </div>
      <div className="flex items-center gap-2 shrink-0">
        <Link
          href={`/users/${friend.userId}/profile`}
          className="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300"
          aria-label={`View ${friend.displayName}'s profile`}
        >
          View profile
        </Link>
        {onRemove && (
          <button
            onClick={() => onRemove(friend.userId)}
            disabled={isRemoving}
            className="text-sm text-red-500 hover:text-red-600 disabled:opacity-50"
            aria-label={`Remove ${friend.displayName} as a friend`}
          >
            Remove
          </button>
        )}
      </div>
    </div>
  )
}
