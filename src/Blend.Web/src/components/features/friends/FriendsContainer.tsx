'use client'

import { useState } from 'react'
import { useFriends, useIncomingRequests, useSentRequests, useAcceptFriendRequest, useDeclineFriendRequest, useRemoveFriend } from '@/hooks/useFriends'
import { FriendCard } from './FriendCard'
import { RequestCard } from './RequestCard'
import { SentRequestCard } from './SentRequestCard'
import { UserSearch } from './UserSearch'

type Tab = 'friends' | 'requests' | 'sent'

export function FriendsContainer() {
  const [activeTab, setActiveTab] = useState<Tab>('friends')

  const { data: friendsData, isLoading: friendsLoading, fetchNextPage: fetchNextFriends, hasNextPage: hasMoreFriends } = useFriends()
  const { data: requestsData, isLoading: requestsLoading, fetchNextPage: fetchNextRequests, hasNextPage: hasMoreRequests } = useIncomingRequests()
  const { data: sentData, isLoading: sentLoading, fetchNextPage: fetchNextSent, hasNextPage: hasMoreSent } = useSentRequests()

  const { mutate: accept, isPending: isAccepting } = useAcceptFriendRequest()
  const { mutate: decline, isPending: isDeclining } = useDeclineFriendRequest()
  const { mutate: remove, isPending: isRemoving } = useRemoveFriend()

  const friends = friendsData?.pages.flatMap((p) => p.items) ?? []
  const requests = requestsData?.pages.flatMap((p) => p.items) ?? []
  const sent = sentData?.pages.flatMap((p) => p.items) ?? []

  const tabs: { id: Tab; label: string; count?: number }[] = [
    { id: 'friends', label: 'Friends', count: friends.length || undefined },
    { id: 'requests', label: 'Requests', count: requests.length || undefined },
    { id: 'sent', label: 'Sent' },
  ]

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-gray-100">Friends</h1>

      <div className="mb-6">
        <h2 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
          Add friends
        </h2>
        <UserSearch onRequestSent={() => setActiveTab('sent')} />
      </div>

      <div className="mb-4 flex border-b border-gray-200 dark:border-gray-800" role="tablist">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            role="tab"
            aria-selected={activeTab === tab.id}
            aria-controls={`tabpanel-${tab.id}`}
            id={`tab-${tab.id}`}
            onClick={() => setActiveTab(tab.id)}
            className={`relative px-4 py-2 text-sm font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 ${
              activeTab === tab.id
                ? 'border-b-2 border-primary-600 text-primary-600 dark:text-primary-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
            }`}
          >
            {tab.label}
            {tab.count !== undefined && tab.count > 0 && (
              <span
                className="ml-1.5 inline-flex items-center rounded-full bg-primary-100 px-2 py-0.5 text-xs font-medium text-primary-700"
                aria-label={`${tab.count} ${tab.label.toLowerCase()}`}
              >
                {tab.count}
              </span>
            )}
          </button>
        ))}
      </div>

      <div
        role="tabpanel"
        id="tabpanel-friends"
        aria-labelledby="tab-friends"
        hidden={activeTab !== 'friends'}
      >
        {friendsLoading ? (
          <p className="text-center text-gray-500" aria-label="Loading friends">Loading…</p>
        ) : friends.length === 0 ? (
          <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center dark:border-gray-700">
            <p className="text-gray-500">You have no friends yet. Use the search above to find people.</p>
          </div>
        ) : (
          <ul className="space-y-3" aria-label="Friends list">
            {friends.map((f) => (
              <li key={f.userId}>
                <FriendCard
                  friend={f}
                  onRemove={(uid) => remove(uid)}
                  isRemoving={isRemoving}
                />
              </li>
            ))}
          </ul>
        )}
        {hasMoreFriends && (
          <button
            onClick={() => void fetchNextFriends()}
            className="mt-4 w-full rounded-md border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Load more
          </button>
        )}
      </div>

      <div
        role="tabpanel"
        id="tabpanel-requests"
        aria-labelledby="tab-requests"
        hidden={activeTab !== 'requests'}
      >
        {requestsLoading ? (
          <p className="text-center text-gray-500" aria-label="Loading requests">Loading…</p>
        ) : requests.length === 0 ? (
          <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center dark:border-gray-700">
            <p className="text-gray-500">No incoming friend requests.</p>
          </div>
        ) : (
          <ul className="space-y-3" aria-label="Friend requests list">
            {requests.map((r) => (
              <li key={r.requestId}>
                <RequestCard
                  request={r}
                  onAccept={(id) => accept(id)}
                  onDecline={(id) => decline(id)}
                  isAccepting={isAccepting}
                  isDeclining={isDeclining}
                />
              </li>
            ))}
          </ul>
        )}
        {hasMoreRequests && (
          <button
            onClick={() => void fetchNextRequests()}
            className="mt-4 w-full rounded-md border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Load more
          </button>
        )}
      </div>

      <div
        role="tabpanel"
        id="tabpanel-sent"
        aria-labelledby="tab-sent"
        hidden={activeTab !== 'sent'}
      >
        {sentLoading ? (
          <p className="text-center text-gray-500" aria-label="Loading sent requests">Loading…</p>
        ) : sent.length === 0 ? (
          <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center dark:border-gray-700">
            <p className="text-gray-500">No pending outgoing requests.</p>
          </div>
        ) : (
          <ul className="space-y-3" aria-label="Sent requests list">
            {sent.map((r) => (
              <li key={r.requestId}>
                <SentRequestCard
                  request={r}
                  onCancel={(id) => decline(id)}
                  isCancelling={isDeclining}
                />
              </li>
            ))}
          </ul>
        )}
        {hasMoreSent && (
          <button
            onClick={() => void fetchNextSent()}
            className="mt-4 w-full rounded-md border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Load more
          </button>
        )}
      </div>
    </div>
  )
}
