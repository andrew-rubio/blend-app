import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { SplashIntro, useSplashIntro } from '@/components/features/SplashIntro'
import { renderHook, act } from '@testing-library/react'

vi.mock('next/link', () => ({
  default: ({ children, href, onClick }: { children: React.ReactNode; href: string; onClick?: () => void }) => (
    <a href={href} onClick={onClick}>{children}</a>
  ),
}))

describe('SplashIntro', () => {
  const onDismiss = vi.fn()

  beforeEach(() => {
    localStorage.clear()
    onDismiss.mockClear()
  })

  it('renders the first step', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    expect(screen.getByTestId('splash-intro')).toBeDefined()
    expect(screen.getByText('Discover Recipes')).toBeDefined()
  })

  it('has a close button', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    const closeBtn = screen.getByTestId('splash-close')
    expect(closeBtn).toBeDefined()
  })

  it('calls onDismiss and sets localStorage when closed', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    fireEvent.click(screen.getByTestId('splash-close'))
    expect(onDismiss).toHaveBeenCalledTimes(1)
    expect(localStorage.getItem('blend-intro-seen')).toBe('true')
  })

  it('navigates through steps with Next button', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    expect(screen.getByText('Discover Recipes')).toBeDefined()

    fireEvent.click(screen.getByText('Next'))
    expect(screen.getByText('Create & Share')).toBeDefined()

    fireEvent.click(screen.getByText('Next'))
    expect(screen.getByText('Connect with Chefs')).toBeDefined()
  })

  it('shows Back button on steps after the first', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    expect(screen.queryByText('Back')).toBeNull()

    fireEvent.click(screen.getByText('Next'))
    expect(screen.getByText('Back')).toBeDefined()
  })

  it('goes back when Back button is clicked', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    fireEvent.click(screen.getByText('Next'))
    expect(screen.getByText('Create & Share')).toBeDefined()

    fireEvent.click(screen.getByText('Back'))
    expect(screen.getByText('Discover Recipes')).toBeDefined()
  })

  it('shows registration options on the last step', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    // Navigate to last step (4 steps: 0, 1, 2, 3)
    fireEvent.click(screen.getByText('Next'))
    fireEvent.click(screen.getByText('Next'))
    fireEvent.click(screen.getByText('Next'))

    expect(screen.getByText('Create an account')).toBeDefined()
    expect(screen.getByText('Sign in')).toBeDefined()
    expect(screen.getByText('Continue as guest')).toBeDefined()
  })

  it('calls onDismiss when Continue as guest is clicked', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    fireEvent.click(screen.getByText('Next'))
    fireEvent.click(screen.getByText('Next'))
    fireEvent.click(screen.getByText('Next'))

    fireEvent.click(screen.getByText('Continue as guest'))
    expect(onDismiss).toHaveBeenCalledTimes(1)
  })

  it('renders step indicators', () => {
    render(<SplashIntro onDismiss={onDismiss} />)
    const indicators = screen.getAllByRole('tab')
    expect(indicators).toHaveLength(4)
  })
})

describe('useSplashIntro', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('shows intro when localStorage flag is not set', () => {
    const { result } = renderHook(() => useSplashIntro())
    expect(result.current.isVisible).toBe(true)
  })

  it('hides intro when localStorage flag is set', () => {
    localStorage.setItem('blend-intro-seen', 'true')
    const { result } = renderHook(() => useSplashIntro())
    expect(result.current.isVisible).toBe(false)
  })

  it('dismiss sets localStorage and hides intro', () => {
    const { result } = renderHook(() => useSplashIntro())
    act(() => {
      result.current.dismiss()
    })
    expect(result.current.isVisible).toBe(false)
    expect(localStorage.getItem('blend-intro-seen')).toBe('true')
  })

  it('show makes intro visible', () => {
    localStorage.setItem('blend-intro-seen', 'true')
    const { result } = renderHook(() => useSplashIntro())
    expect(result.current.isVisible).toBe(false)
    act(() => {
      result.current.show()
    })
    expect(result.current.isVisible).toBe(true)
  })
})
