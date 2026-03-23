import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { NotificationBell } from '@/components/features/notifications/NotificationBell'

vi.mock('next/link', () => ({
  default: ({ children, href, ...props }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...props}>{children}</a>
  ),
}))

vi.mock('@/hooks/useNotifications', () => ({
  usePollUnreadCount: vi.fn(() => vi.fn()),
}))

vi.mock('@/hooks/useAdaptivePolling', () => ({
  useAdaptivePolling: vi.fn(),
}))

vi.mock('@/stores/notificationStore', () => ({
  useNotificationStore: vi.fn(),
}))

import { useNotificationStore } from '@/stores/notificationStore'

const mockUseNotificationStore = vi.mocked(useNotificationStore)

describe('NotificationBell', () => {
  beforeEach(() => {
    mockUseNotificationStore.mockReturnValue(0 as unknown as ReturnType<typeof useNotificationStore>)
  })

  it('renders bell link with correct href', () => {
    render(<NotificationBell />)
    const link = screen.getByRole('link')
    expect(link.getAttribute('href')).toBe('/notifications')
  })

  it('has correct aria-label with no unread notifications', () => {
    render(<NotificationBell />)
    expect(screen.getByRole('link', { name: 'Notifications' })).toBeTruthy()
  })

  it('has correct aria-label with unread notifications', () => {
    mockUseNotificationStore.mockReturnValue(3 as unknown as ReturnType<typeof useNotificationStore>)
    render(<NotificationBell />)
    expect(screen.getByRole('link', { name: /Notifications, 3 unread/i })).toBeTruthy()
  })

  it('does not show badge when count is 0', () => {
    render(<NotificationBell />)
    expect(screen.queryByText('0')).toBeNull()
  })

  it('shows badge with count when unread > 0', () => {
    mockUseNotificationStore.mockReturnValue(5 as unknown as ReturnType<typeof useNotificationStore>)
    render(<NotificationBell />)
    expect(screen.getByText('5')).toBeTruthy()
  })

  it('shows 99+ when unread count exceeds 99', () => {
    mockUseNotificationStore.mockReturnValue(100 as unknown as ReturnType<typeof useNotificationStore>)
    render(<NotificationBell />)
    expect(screen.getByText('99+')).toBeTruthy()
  })
})
