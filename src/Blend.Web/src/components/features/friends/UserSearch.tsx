'use client'

import { useState, useEffect, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useUserSearch, useSendFriendRequest, friendsQueryKeys } from '@/hooks/useFriends'

interface UserSearchProps {
  onRequestSent?: () => void
}

export function UserSearch({ onRequestSent }: UserSearchProps) {
  const [inputValue, setInputValue] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (timerRef.current) clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => {
      setDebouncedQuery(inputValue.trim())
    }, 300)
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current)
    }
  }, [inputValue])

  const { data, isLoading } = useUserSearch(debouncedQuery)
  const { mutate: sendRequest, isPending: isSending } = useSendFriendRequest()

  const handleAdd = (userId: string) => {
    sendRequest(userId, {
      onSuccess: () => {
        void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.search(debouncedQuery) })
        onRequestSent?.()
      },
    })
  }

  return (
    <div>
      <label htmlFor="user-search" className="sr-only">
        Search for friends by display name
      </label>
      <input
        id="user-search"
        type="search"
        value={inputValue}
        onChange={(e) => setInputValue(e.target.value)}
        placeholder="Search by display name…"
        className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
        aria-label="Search for friends by display name"
      />

      {isLoading && debouncedQuery && (
        <p className="mt-2 text-sm text-gray-500" aria-live="polite">
          Searching…
        </p>
      )}

      {data && data.items.length === 0 && debouncedQuery && !isLoading && (
        <p className="mt-3 text-sm text-gray-500" aria-live="polite">
          No users found for "{debouncedQuery}".
        </p>
      )}

      {data && data.items.length > 0 && (
        <ul className="mt-3 space-y-2" aria-label="Search results">
          {data.items.map((user) => (
            <li
              key={user.userId}
              className="flex items-center gap-3 rounded-lg border border-gray-200 bg-white p-3 dark:border-gray-800 dark:bg-gray-950"
            >
              <div
                className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-100 text-primary-700 font-semibold shrink-0"
                aria-hidden="true"
              >
                {user.avatarUrl ? (
                  <img src={user.avatarUrl} alt="" className="h-10 w-10 rounded-full object-cover" />
                ) : (
                  user.displayName.charAt(0).toUpperCase()
                )}
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-medium text-gray-900 dark:text-gray-100 truncate">{user.displayName}</p>
                <p className="text-xs text-gray-500">{user.recipeCount} recipes</p>
              </div>
              <div className="shrink-0">
                {user.connectionStatus === 'none' && (
                  <button
                    onClick={() => handleAdd(user.userId)}
                    disabled={isSending}
                    className="rounded-md bg-primary-600 px-3 py-1 text-sm font-medium text-white hover:bg-primary-700 disabled:opacity-50"
                    aria-label={`Add ${user.displayName} as a friend`}
                  >
                    Add friend
                  </button>
                )}
                {user.connectionStatus === 'pending_sent' && (
                  <span
                    className="inline-flex items-center rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-800"
                    aria-label={`Friend request to ${user.displayName} is pending`}
                  >
                    Pending
                  </span>
                )}
                {user.connectionStatus === 'pending_received' && (
                  <span
                    className="inline-flex items-center rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800"
                    aria-label={`${user.displayName} sent you a friend request`}
                  >
                    Requested you
                  </span>
                )}
                {user.connectionStatus === 'friends' && (
                  <span
                    className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800"
                    aria-label={`${user.displayName} is already your friend`}
                  >
                    Friends
                  </span>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
