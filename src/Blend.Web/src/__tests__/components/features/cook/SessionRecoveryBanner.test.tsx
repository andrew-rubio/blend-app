import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SessionRecoveryBanner } from '@/components/features/cook/SessionRecoveryBanner'

describe('SessionRecoveryBanner', () => {
  const mockOnResume = vi.fn()
  const mockOnDismiss = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders banner with recovery message', () => {
    render(<SessionRecoveryBanner onResume={mockOnResume} onDismiss={mockOnDismiss} />)
    expect(screen.getByTestId('session-recovery-banner')).toBeDefined()
    expect(screen.getByText('You have a paused cooking session. Resume it?')).toBeDefined()
  })

  it('calls onResume when Resume button clicked', () => {
    render(<SessionRecoveryBanner onResume={mockOnResume} onDismiss={mockOnDismiss} />)
    fireEvent.click(screen.getByTestId('session-recovery-resume'))
    expect(mockOnResume).toHaveBeenCalled()
  })

  it('calls onDismiss when Dismiss button clicked', () => {
    render(<SessionRecoveryBanner onResume={mockOnResume} onDismiss={mockOnDismiss} />)
    fireEvent.click(screen.getByTestId('session-recovery-dismiss'))
    expect(mockOnDismiss).toHaveBeenCalled()
  })

  it('has role="alert" for accessibility', () => {
    render(<SessionRecoveryBanner onResume={mockOnResume} onDismiss={mockOnDismiss} />)
    expect(screen.getByRole('alert')).toBeDefined()
  })
})
