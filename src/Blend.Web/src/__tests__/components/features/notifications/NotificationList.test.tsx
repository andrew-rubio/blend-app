import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { NotificationList } from '@/components/features/notifications/NotificationList'
import type { ApiNotificationItem } from '@/types'

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

vi.mock('@tanstack/react-query', () => ({
  useQueryClient: () => ({ invalidateQueries: vi.fn() }),
}))

vi.mock('@/hooks/useNotifications', () => ({
  useNotifications: vi.fn(),
  useMarkNotificationRead: vi.fn(),
  useMarkAllNotificationsRead: vi.fn(),
  notificationQueryKeys: {
    all: ['notifications'],
    list: () => ['notifications', 'list'],
    unreadCount: () => ['notifications', 'unread-count'],
  },
}))

import {
  useNotifications,
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
} from '@/hooks/useNotifications'

const mockUseNotifications = vi.mocked(useNotifications)
const mockUseMarkNotificationRead = vi.mocked(useMarkNotificationRead)
const mockUseMarkAllNotificationsRead = vi.mocked(useMarkAllNotificationsRead)

const mockNotification: ApiNotificationItem = {
  id: 'n1',
  type: 'friendRequestReceived',
  actorDisplayName: 'Alice',
  isRead: false,
  createdAt: new Date(Date.now() - 60_000).toISOString(),
}

function setupDefaultMocks() {
  mockUseNotifications.mockReturnValue({
    data: undefined,
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
    isFetchingNextPage: false,
  } as unknown as ReturnType<typeof useNotifications>)
  mockUseMarkNotificationRead.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useMarkNotificationRead>)
  mockUseMarkAllNotificationsRead.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useMarkAllNotificationsRead>)
}

describe('NotificationList', () => {
  beforeEach(() => {
    setupDefaultMocks()
  })

  it('renders page heading', () => {
    render(<NotificationList />)
    expect(screen.getByRole('heading', { name: 'Notifications' })).toBeTruthy()
  })

  it('shows loading state', () => {
    mockUseNotifications.mockReturnValue({
      data: undefined,
      isLoading: true,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useNotifications>)
    render(<NotificationList />)
    expect(screen.getByLabelText('Loading notifications')).toBeTruthy()
  })

  it('shows empty state when no notifications', () => {
    render(<NotificationList />)
    expect(screen.getByText(/You have no notifications yet/i)).toBeTruthy()
  })

  it('shows notifications list', () => {
    mockUseNotifications.mockReturnValue({
      data: { pages: [{ items: [mockNotification], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useNotifications>)
    render(<NotificationList />)
    expect(screen.getByText(/Alice sent you a friend request/i)).toBeTruthy()
  })

  it('shows mark all read button when there are unread notifications', () => {
    mockUseNotifications.mockReturnValue({
      data: { pages: [{ items: [mockNotification], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useNotifications>)
    render(<NotificationList />)
    expect(screen.getByRole('button', { name: /Mark all notifications as read/i })).toBeTruthy()
  })

  it('calls markAllRead when button is clicked', () => {
    const markAllRead = vi.fn()
    mockUseMarkAllNotificationsRead.mockReturnValue({ mutate: markAllRead, isPending: false } as unknown as ReturnType<typeof useMarkAllNotificationsRead>)
    mockUseNotifications.mockReturnValue({
      data: { pages: [{ items: [mockNotification], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useNotifications>)
    render(<NotificationList />)
    fireEvent.click(screen.getByRole('button', { name: /Mark all notifications as read/i }))
    expect(markAllRead).toHaveBeenCalled()
  })

  it('does not show mark all read button when all are read', () => {
    const readNotif = { ...mockNotification, isRead: true }
    mockUseNotifications.mockReturnValue({
      data: { pages: [{ items: [readNotif], hasNextPage: false }], pageParams: [] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useNotifications>)
    render(<NotificationList />)
    expect(screen.queryByRole('button', { name: /Mark all/i })).toBeNull()
  })
})
