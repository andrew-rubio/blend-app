import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { NotificationItem } from '@/components/features/notifications/NotificationItem'
import type { ApiNotificationItem } from '@/types'

const mockPush = vi.fn()

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

const baseNotification: ApiNotificationItem = {
  id: 'n1',
  type: 'friendRequestReceived',
  actorDisplayName: 'Alice',
  isRead: false,
  createdAt: new Date(Date.now() - 2 * 60_000).toISOString(), // 2 minutes ago
}

describe('NotificationItem', () => {
  beforeEach(() => {
    mockPush.mockReset()
  })

  it('renders friendRequestReceived message', () => {
    render(<NotificationItem notification={baseNotification} onMarkRead={vi.fn()} />)
    expect(screen.getByText(/Alice sent you a friend request/i)).toBeTruthy()
  })

  it('renders friendRequestAccepted message', () => {
    const notif: ApiNotificationItem = { ...baseNotification, type: 'friendRequestAccepted', actorDisplayName: 'Bob' }
    render(<NotificationItem notification={notif} onMarkRead={vi.fn()} />)
    expect(screen.getByText(/Bob accepted your friend request/i)).toBeTruthy()
  })

  it('renders recipeLiked message with recipe title', () => {
    const notif: ApiNotificationItem = { ...baseNotification, type: 'recipeLiked', actorDisplayName: 'Carol', recipeTitle: 'Pasta' }
    render(<NotificationItem notification={notif} onMarkRead={vi.fn()} />)
    expect(screen.getByText(/Carol liked your recipe "Pasta"/i)).toBeTruthy()
  })

  it('renders recipePublished message', () => {
    const notif: ApiNotificationItem = { ...baseNotification, type: 'recipePublished', recipeTitle: 'Steak' }
    render(<NotificationItem notification={notif} onMarkRead={vi.fn()} />)
    expect(screen.getByText(/Your recipe "Steak" has been published/i)).toBeTruthy()
  })

  it('calls onMarkRead when unread notification is clicked', () => {
    const onMarkRead = vi.fn()
    render(<NotificationItem notification={baseNotification} onMarkRead={onMarkRead} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onMarkRead).toHaveBeenCalledWith('n1')
  })

  it('does not call onMarkRead when notification is already read', () => {
    const onMarkRead = vi.fn()
    const readNotif = { ...baseNotification, isRead: true }
    render(<NotificationItem notification={readNotif} onMarkRead={onMarkRead} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onMarkRead).not.toHaveBeenCalled()
  })

  it('navigates to /friends?tab=requests for friendRequestReceived', () => {
    render(<NotificationItem notification={baseNotification} onMarkRead={vi.fn()} />)
    fireEvent.click(screen.getByRole('button'))
    expect(mockPush).toHaveBeenCalledWith('/friends?tab=requests')
  })

  it('navigates to user profile for friendRequestAccepted', () => {
    const notif: ApiNotificationItem = { ...baseNotification, type: 'friendRequestAccepted', targetUserId: 'u99' }
    render(<NotificationItem notification={notif} onMarkRead={vi.fn()} />)
    fireEvent.click(screen.getByRole('button'))
    expect(mockPush).toHaveBeenCalledWith('/users/u99/profile')
  })

  it('navigates to recipe for recipeLiked', () => {
    const notif: ApiNotificationItem = { ...baseNotification, type: 'recipeLiked', recipeId: 'recipe-5' }
    render(<NotificationItem notification={notif} onMarkRead={vi.fn()} />)
    fireEvent.click(screen.getByRole('button'))
    expect(mockPush).toHaveBeenCalledWith('/recipes/recipe-5')
  })

  it('displays relative time', () => {
    render(<NotificationItem notification={baseNotification} onMarkRead={vi.fn()} />)
    expect(screen.getByText('2m ago')).toBeTruthy()
  })
})
