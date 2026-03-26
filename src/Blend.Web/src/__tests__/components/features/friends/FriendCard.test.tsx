import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FriendCard } from '@/components/features/friends/FriendCard'
import type { FriendItem } from '@/types'

vi.mock('next/link', () => ({
  default: ({ children, href, ...props }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...props}>{children}</a>
  ),
}))

const mockFriend: FriendItem = {
  userId: 'user-1',
  displayName: 'Alice Chef',
  recipeCount: 5,
  connectedAt: '2024-01-01T00:00:00Z',
}

describe('FriendCard', () => {
  it('renders display name and recipe count', () => {
    render(<FriendCard friend={mockFriend} />)
    expect(screen.getByText('Alice Chef')).toBeTruthy()
    expect(screen.getByText('5 recipes')).toBeTruthy()
  })

  it('renders singular recipe count correctly', () => {
    render(<FriendCard friend={{ ...mockFriend, recipeCount: 1 }} />)
    expect(screen.getByText('1 recipe')).toBeTruthy()
  })

  it('renders view profile link with correct href', () => {
    render(<FriendCard friend={mockFriend} />)
    const link = screen.getByRole('link', { name: /View Alice Chef's profile/i })
    expect(link.getAttribute('href')).toBe('/users/user-1/profile')
  })

  it('renders remove button when onRemove provided', () => {
    const onRemove = vi.fn()
    render(<FriendCard friend={mockFriend} onRemove={onRemove} />)
    const btn = screen.getByRole('button', { name: /Remove Alice Chef/i })
    expect(btn).toBeTruthy()
  })

  it('calls onRemove with userId when clicked', () => {
    const onRemove = vi.fn()
    render(<FriendCard friend={mockFriend} onRemove={onRemove} />)
    fireEvent.click(screen.getByRole('button', { name: /Remove Alice Chef/i }))
    expect(onRemove).toHaveBeenCalledWith('user-1')
  })

  it('disables remove button when isRemoving is true', () => {
    const onRemove = vi.fn()
    render(<FriendCard friend={mockFriend} onRemove={onRemove} isRemoving={true} />)
    const btn = screen.getByRole('button', { name: /Remove Alice Chef/i })
    expect(btn).toBeDisabled()
  })

  it('does not render remove button when onRemove not provided', () => {
    render(<FriendCard friend={mockFriend} />)
    expect(screen.queryByRole('button')).toBeNull()
  })
})
