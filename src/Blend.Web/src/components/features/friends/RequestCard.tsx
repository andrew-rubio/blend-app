'use client'

import type { FriendRequestItem } from '@/types'

interface RequestCardProps {
  request: FriendRequestItem
  onAccept: (requestId: string) => void
  onDecline: (requestId: string) => void
  isAccepting?: boolean
  isDeclining?: boolean
}

export function RequestCard({ request, onAccept, onDecline, isAccepting, isDeclining }: RequestCardProps) {
  return (
    <div className="flex items-center gap-4 rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-950">
      <div
        className="flex h-12 w-12 items-center justify-center rounded-full bg-primary-100 text-primary-700 font-semibold text-lg shrink-0"
        aria-hidden="true"
      >
        {request.avatarUrl ? (
          <img src={request.avatarUrl} alt="" className="h-12 w-12 rounded-full object-cover" />
        ) : (
          request.displayName.charAt(0).toUpperCase()
        )}
      </div>
      <div className="flex-1 min-w-0">
        <p className="font-semibold text-gray-900 dark:text-gray-100 truncate">{request.displayName}</p>
        <p className="text-sm text-gray-500 dark:text-gray-400">Wants to connect</p>
      </div>
      <div className="flex items-center gap-2 shrink-0">
        <button
          onClick={() => onAccept(request.requestId)}
          disabled={isAccepting || isDeclining}
          className="rounded-md bg-primary-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-primary-700 disabled:opacity-50"
          aria-label={`Accept friend request from ${request.displayName}`}
        >
          Accept
        </button>
        <button
          onClick={() => onDecline(request.requestId)}
          disabled={isAccepting || isDeclining}
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label={`Decline friend request from ${request.displayName}`}
        >
          Decline
        </button>
      </div>
    </div>
  )
}
