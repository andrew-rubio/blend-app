import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SentRequestCard } from '@/components/features/friends/SentRequestCard'
import type { FriendRequestItem } from '@/types'

const mockRequest: FriendRequestItem = {
  requestId: 'req-5',
  userId: 'user-5',
  displayName: 'Carol Cook',
  sentAt: '2024-01-01T00:00:00Z',
}

describe('SentRequestCard', () => {
  it('renders display name', () => {
    render(<SentRequestCard request={mockRequest} />)
    expect(screen.getByText('Carol Cook')).toBeTruthy()
  })

  it('renders pending badge', () => {
    render(<SentRequestCard request={mockRequest} />)
    expect(screen.getByText('Pending')).toBeTruthy()
  })

  it('renders cancel button when onCancel provided', () => {
    const onCancel = vi.fn()
    render(<SentRequestCard request={mockRequest} onCancel={onCancel} />)
    expect(screen.getByRole('button', { name: /Cancel friend request to Carol Cook/i })).toBeTruthy()
  })

  it('calls onCancel with requestId when clicked', () => {
    const onCancel = vi.fn()
    render(<SentRequestCard request={mockRequest} onCancel={onCancel} />)
    fireEvent.click(screen.getByRole('button', { name: /Cancel/i }))
    expect(onCancel).toHaveBeenCalledWith('req-5')
  })

  it('disables cancel button when isCancelling', () => {
    render(<SentRequestCard request={mockRequest} onCancel={vi.fn()} isCancelling={true} />)
    expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled()
  })

  it('does not render cancel button when onCancel not provided', () => {
    render(<SentRequestCard request={mockRequest} />)
    expect(screen.queryByRole('button')).toBeNull()
  })
})
