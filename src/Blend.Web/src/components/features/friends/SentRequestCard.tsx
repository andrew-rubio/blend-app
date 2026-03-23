'use client'

import type { FriendRequestItem } from '@/types'

interface SentRequestCardProps {
  request: FriendRequestItem
  onCancel?: (requestId: string) => void
  isCancelling?: boolean
}

export function SentRequestCard({ request, onCancel, isCancelling }: SentRequestCardProps) {
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
        <span className="inline-flex items-center rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-800">
          Pending
        </span>
      </div>
      {onCancel && (
        <button
          onClick={() => onCancel(request.requestId)}
          disabled={isCancelling}
          className="text-sm text-gray-500 hover:text-gray-700 disabled:opacity-50 shrink-0"
          aria-label={`Cancel friend request to ${request.displayName}`}
        >
          Cancel
        </button>
      )}
    </div>
  )
}
