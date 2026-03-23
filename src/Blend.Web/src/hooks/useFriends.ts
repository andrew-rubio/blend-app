import { useQuery, useMutation, useInfiniteQuery, useQueryClient } from '@tanstack/react-query'
import {
  getFriendsApi,
  getIncomingRequestsApi,
  getSentRequestsApi,
  sendFriendRequestApi,
  acceptFriendRequestApi,
  declineFriendRequestApi,
  removeFriendApi,
  searchUsersApi,
} from '@/lib/api/friends'
import type { FriendsPageResponse, FriendRequestsPageResponse, UserSearchPageResponse } from '@/types'

const FRIENDS_STALE_TIME = 60_000

export const friendsQueryKeys = {
  all: ['friends'] as const,
  list: () => [...friendsQueryKeys.all, 'list'] as const,
  incoming: () => [...friendsQueryKeys.all, 'incoming'] as const,
  sent: () => [...friendsQueryKeys.all, 'sent'] as const,
  search: (query: string) => [...friendsQueryKeys.all, 'search', query] as const,
}

export function useFriends() {
  return useInfiniteQuery<FriendsPageResponse, { message: string; status: number }>({
    queryKey: friendsQueryKeys.list(),
    queryFn: ({ pageParam }) => getFriendsApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.nextCursor : undefined),
    staleTime: FRIENDS_STALE_TIME,
  })
}

export function useIncomingRequests() {
  return useInfiniteQuery<FriendRequestsPageResponse, { message: string; status: number }>({
    queryKey: friendsQueryKeys.incoming(),
    queryFn: ({ pageParam }) => getIncomingRequestsApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.nextCursor : undefined),
    staleTime: FRIENDS_STALE_TIME,
  })
}

export function useSentRequests() {
  return useInfiniteQuery<FriendRequestsPageResponse, { message: string; status: number }>({
    queryKey: friendsQueryKeys.sent(),
    queryFn: ({ pageParam }) => getSentRequestsApi(pageParam as string | undefined),
    initialPageParam: undefined,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.nextCursor : undefined),
    staleTime: FRIENDS_STALE_TIME,
  })
}

export function useUserSearch(query: string) {
  return useQuery<UserSearchPageResponse, { message: string; status: number }>({
    queryKey: friendsQueryKeys.search(query),
    queryFn: () => searchUsersApi(query),
    staleTime: 30_000,
    enabled: query.trim().length >= 1,
  })
}

export function useSendFriendRequest() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (targetUserId: string) => sendFriendRequestApi(targetUserId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.sent() })
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.all })
    },
  })
}

export function useAcceptFriendRequest() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (requestId: string) => acceptFriendRequestApi(requestId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.list() })
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.incoming() })
    },
  })
}

export function useDeclineFriendRequest() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (requestId: string) => declineFriendRequestApi(requestId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.incoming() })
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.sent() })
    },
  })
}

export function useRemoveFriend() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (friendUserId: string) => removeFriendApi(friendUserId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: friendsQueryKeys.list() })
    },
  })
}
