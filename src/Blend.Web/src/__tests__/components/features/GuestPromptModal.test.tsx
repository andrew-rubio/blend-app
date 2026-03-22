import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { renderHook, act } from '@testing-library/react'
import React from 'react'
import { GuestPromptModal, useGuestPrompt } from '@/components/features/GuestPromptModal'

vi.mock('next/link', () => ({
  default: ({ children, href, onClick }: { children: React.ReactNode; href: string; onClick?: () => void }) => (
    <a href={href} onClick={onClick}>{children}</a>
  ),
}))

describe('GuestPromptModal', () => {
  const onClose = vi.fn()

  beforeEach(() => {
    onClose.mockClear()
  })

  it('does not render when isOpen is false', () => {
    render(<GuestPromptModal isOpen={false} onClose={onClose} />)
    expect(screen.queryByTestId('guest-prompt-modal')).toBeNull()
  })

  it('renders when isOpen is true', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />)
    expect(screen.getByTestId('guest-prompt-modal')).toBeDefined()
    expect(screen.getByText('Sign in to continue')).toBeDefined()
  })

  it('shows default message when no message is provided', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />)
    expect(screen.getByText('Create a free account or sign in to access this feature.')).toBeDefined()
  })

  it('shows custom message when provided', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} message="Please sign in to like recipes." />)
    expect(screen.getByText('Please sign in to like recipes.')).toBeDefined()
  })

  it('renders Register and Sign in links', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />)
    expect(screen.getByText('Create a free account')).toBeDefined()
    expect(screen.getByText('Sign in')).toBeDefined()
  })

  it('calls onClose when Maybe later is clicked', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />)
    fireEvent.click(screen.getByText('Maybe later'))
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onClose when backdrop is clicked', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />)
    const backdrop = screen.getByTestId('guest-prompt-modal')
    fireEvent.click(backdrop)
    expect(onClose).toHaveBeenCalledTimes(1)
  })
})

describe('useGuestPrompt', () => {
  it('starts closed', () => {
    const { result } = renderHook(() => useGuestPrompt())
    expect(result.current.isOpen).toBe(false)
    expect(result.current.message).toBeUndefined()
  })

  it('opens with message when prompt is called', () => {
    const { result } = renderHook(() => useGuestPrompt())
    act(() => {
      result.current.prompt('You must sign in to like recipes.')
    })
    expect(result.current.isOpen).toBe(true)
    expect(result.current.message).toBe('You must sign in to like recipes.')
  })

  it('closes when close is called', () => {
    const { result } = renderHook(() => useGuestPrompt())
    act(() => {
      result.current.prompt()
    })
    act(() => {
      result.current.close()
    })
    expect(result.current.isOpen).toBe(false)
  })
})
