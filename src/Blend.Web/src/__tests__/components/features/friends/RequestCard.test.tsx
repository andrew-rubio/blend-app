import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { RequestCard } from '@/components/features/friends/RequestCard'
import type { FriendRequestItem } from '@/types'

const mockRequest: FriendRequestItem = {
  requestId: 'req-1',
  userId: 'user-2',
  displayName: 'Bob Baker',
  sentAt: '2024-01-01T00:00:00Z',
}

describe('RequestCard', () => {
  it('renders display name', () => {
    render(<RequestCard request={mockRequest} onAccept={vi.fn()} onDecline={vi.fn()} />)
    expect(screen.getByText('Bob Baker')).toBeTruthy()
  })

  it('renders accept and decline buttons', () => {
    render(<RequestCard request={mockRequest} onAccept={vi.fn()} onDecline={vi.fn()} />)
    expect(screen.getByRole('button', { name: /Accept friend request from Bob Baker/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Decline friend request from Bob Baker/i })).toBeTruthy()
  })

  it('calls onAccept with requestId', () => {
    const onAccept = vi.fn()
    render(<RequestCard request={mockRequest} onAccept={onAccept} onDecline={vi.fn()} />)
    fireEvent.click(screen.getByRole('button', { name: /Accept/i }))
    expect(onAccept).toHaveBeenCalledWith('req-1')
  })

  it('calls onDecline with requestId', () => {
    const onDecline = vi.fn()
    render(<RequestCard request={mockRequest} onAccept={vi.fn()} onDecline={onDecline} />)
    fireEvent.click(screen.getByRole('button', { name: /Decline/i }))
    expect(onDecline).toHaveBeenCalledWith('req-1')
  })

  it('disables buttons when isAccepting', () => {
    render(<RequestCard request={mockRequest} onAccept={vi.fn()} onDecline={vi.fn()} isAccepting={true} />)
    expect(screen.getByRole('button', { name: /Accept/i })).toBeDisabled()
    expect(screen.getByRole('button', { name: /Decline/i })).toBeDisabled()
  })

  it('disables buttons when isDeclining', () => {
    render(<RequestCard request={mockRequest} onAccept={vi.fn()} onDecline={vi.fn()} isDeclining={true} />)
    expect(screen.getByRole('button', { name: /Accept/i })).toBeDisabled()
    expect(screen.getByRole('button', { name: /Decline/i })).toBeDisabled()
  })
})
