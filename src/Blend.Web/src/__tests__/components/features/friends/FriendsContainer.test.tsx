import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FriendsContainer } from '@/components/features/friends/FriendsContainer'
import type { FriendItem, FriendRequestItem } from '@/types'

vi.mock('@/hooks/useFriends', () => ({
  useFriends: vi.fn(),
  useIncomingRequests: vi.fn(),
  useSentRequests: vi.fn(),
  useAcceptFriendRequest: vi.fn(),
  useDeclineFriendRequest: vi.fn(),
  useRemoveFriend: vi.fn(),
}))

vi.mock('@/components/features/friends/UserSearch', () => ({
  UserSearch: ({ onRequestSent: _onRequestSent }: { onRequestSent?: () => void }) => (
    <div data-testid="user-search">UserSearch</div>
  ),
}))

import {
  useFriends,
  useIncomingRequests,
  useSentRequests,
  useAcceptFriendRequest,
  useDeclineFriendRequest,
  useRemoveFriend,
} from '@/hooks/useFriends'

const mockUseFriends = vi.mocked(useFriends)
const mockUseIncomingRequests = vi.mocked(useIncomingRequests)
const mockUseSentRequests = vi.mocked(useSentRequests)
const mockUseAcceptFriendRequest = vi.mocked(useAcceptFriendRequest)
const mockUseDeclineFriendRequest = vi.mocked(useDeclineFriendRequest)
const mockUseRemoveFriend = vi.mocked(useRemoveFriend)

const mockFriend: FriendItem = {
  userId: 'u1',
  displayName: 'Alice Chef',
  recipeCount: 3,
  connectedAt: '2024-01-01T00:00:00Z',
}

const mockRequest: FriendRequestItem = {
  requestId: 'r1',
  userId: 'u2',
  displayName: 'Bob Baker',
  sentAt: '2024-01-01T00:00:00Z',
}

function setupDefaultMocks() {
  mockUseFriends.mockReturnValue({
    data: { pages: [{ items: [], hasNextPage: false }], pageParams: [] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
  } as unknown as ReturnType<typeof useFriends>)
  mockUseIncomingRequests.mockReturnValue({
    data: { pages: [{ items: [], hasNextPage: false }], pageParams: [] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
  } as unknown as ReturnType<typeof useIncomingRequests>)
  mockUseSentRequests.mockReturnValue({
    data: { pages: [{ items: [], hasNextPage: false }], pageParams: [] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
  } as unknown as ReturnType<typeof useSentRequests>)
  mockUseAcceptFriendRequest.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useAcceptFriendRequest>)
  mockUseDeclineFriendRequest.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useDeclineFriendRequest>)
  mockUseRemoveFriend.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useRemoveFriend>)
}

describe('FriendsContainer', () => {
  beforeEach(() => {
    setupDefaultMocks()
  })

  it('renders page heading', () => {
    render(<FriendsContainer />)
    expect(screen.getByRole('heading', { name: 'Friends' })).toBeTruthy()
  })

  it('renders tab buttons', () => {
    render(<FriendsContainer />)
    expect(screen.getByRole('tab', { name: /Friends/i })).toBeTruthy()
    expect(screen.getByRole('tab', { name: /Requests/i })).toBeTruthy()
    expect(screen.getByRole('tab', { name: /Sent/i })).toBeTruthy()
  })

  it('shows empty state for friends tab by default', () => {
    render(<FriendsContainer />)
    expect(screen.getByText(/You have no friends yet/i)).toBeTruthy()
  })

  it('shows friends list when data exists', () => {
    mockUseFriends.mockReturnValue({
      data: { pages: [{ items: [mockFriend], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
    } as unknown as ReturnType<typeof useFriends>)
    render(<FriendsContainer />)
    expect(screen.getByText('Alice Chef')).toBeTruthy()
  })

  it('switches to requests tab on click', () => {
    mockUseIncomingRequests.mockReturnValue({
      data: { pages: [{ items: [mockRequest], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
    } as unknown as ReturnType<typeof useIncomingRequests>)
    render(<FriendsContainer />)
    fireEvent.click(screen.getByRole('tab', { name: /Requests/i }))
    expect(screen.getByText('Bob Baker')).toBeTruthy()
  })

  it('shows loading state for friends', () => {
    mockUseFriends.mockReturnValue({
      data: undefined,
      isLoading: true,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
    } as unknown as ReturnType<typeof useFriends>)
    render(<FriendsContainer />)
    expect(screen.getByLabelText('Loading friends')).toBeTruthy()
  })
})
