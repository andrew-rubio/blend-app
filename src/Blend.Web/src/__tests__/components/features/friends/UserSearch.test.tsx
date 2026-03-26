import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { UserSearch } from '@/components/features/friends/UserSearch'
import type { UserSearchPageResponse } from '@/types'

vi.mock('@tanstack/react-query', () => ({
  useQueryClient: () => ({ invalidateQueries: vi.fn() }),
}))

vi.mock('@/hooks/useFriends', () => ({
  useUserSearch: vi.fn(),
  useSendFriendRequest: vi.fn(),
  friendsQueryKeys: {
    all: ['friends'],
    list: () => ['friends', 'list'],
    incoming: () => ['friends', 'incoming'],
    sent: () => ['friends', 'sent'],
    search: (q: string) => ['friends', 'search', q],
  },
}))

import { useUserSearch, useSendFriendRequest } from '@/hooks/useFriends'

const mockUseUserSearch = vi.mocked(useUserSearch)
const mockUseSendFriendRequest = vi.mocked(useSendFriendRequest)

function setupDefaultMocks() {
  mockUseUserSearch.mockReturnValue({
    data: undefined,
    isLoading: false,
    error: null,
  } as unknown as ReturnType<typeof useUserSearch>)
  mockUseSendFriendRequest.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
  } as unknown as ReturnType<typeof useSendFriendRequest>)
}

describe('UserSearch', () => {
  beforeEach(() => {
    setupDefaultMocks()
  })

  it('renders search input', () => {
    render(<UserSearch />)
    expect(screen.getByRole('searchbox')).toBeTruthy()
  })

  it('shows search results with Add friend button for none status', () => {
    const searchData: UserSearchPageResponse = {
      items: [{ userId: 'u1', displayName: 'Alice', recipeCount: 3, connectionStatus: 'none' }],
      nextCursor: undefined,
      hasNextPage: false,
    }
    mockUseUserSearch.mockReturnValue({
      data: searchData,
      isLoading: false,
    } as unknown as ReturnType<typeof useUserSearch>)
    render(<UserSearch />)
    expect(screen.getByText('Alice')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Add Alice as a friend/i })).toBeTruthy()
  })

  it('shows Pending badge for pending_sent status', () => {
    const searchData: UserSearchPageResponse = {
      items: [{ userId: 'u2', displayName: 'Bob', recipeCount: 1, connectionStatus: 'pending_sent' }],
      nextCursor: undefined,
      hasNextPage: false,
    }
    mockUseUserSearch.mockReturnValue({
      data: searchData,
      isLoading: false,
    } as unknown as ReturnType<typeof useUserSearch>)
    render(<UserSearch />)
    expect(screen.getByText('Bob')).toBeTruthy()
    expect(screen.getByText('Pending')).toBeTruthy()
  })

  it('shows Friends badge for friends status', () => {
    const searchData: UserSearchPageResponse = {
      items: [{ userId: 'u3', displayName: 'Carol', recipeCount: 2, connectionStatus: 'friends' }],
      nextCursor: undefined,
      hasNextPage: false,
    }
    mockUseUserSearch.mockReturnValue({
      data: searchData,
      isLoading: false,
    } as unknown as ReturnType<typeof useUserSearch>)
    render(<UserSearch />)
    expect(screen.getByText('Friends')).toBeTruthy()
  })

  it('calls sendRequest when Add friend is clicked', () => {
    const mutate = vi.fn()
    mockUseSendFriendRequest.mockReturnValue({
      mutate,
      isPending: false,
    } as unknown as ReturnType<typeof useSendFriendRequest>)
    const searchData: UserSearchPageResponse = {
      items: [{ userId: 'u1', displayName: 'Alice', recipeCount: 3, connectionStatus: 'none' }],
      nextCursor: undefined,
      hasNextPage: false,
    }
    mockUseUserSearch.mockReturnValue({
      data: searchData,
      isLoading: false,
    } as unknown as ReturnType<typeof useUserSearch>)
    render(<UserSearch />)
    fireEvent.click(screen.getByRole('button', { name: /Add Alice as a friend/i }))
    expect(mutate).toHaveBeenCalledWith('u1', expect.any(Object))
  })
})
