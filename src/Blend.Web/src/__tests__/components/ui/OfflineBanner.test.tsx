import React from 'react'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import { OfflineBanner } from '@/components/ui/OfflineBanner'

describe('OfflineBanner', () => {
  const originalNavigatorOnline = Object.getOwnPropertyDescriptor(navigator, 'onLine')

  function setOnlineStatus(online: boolean) {
    Object.defineProperty(navigator, 'onLine', {
      configurable: true,
      get: () => online,
    })
  }

  beforeEach(() => {
    // Default: online
    setOnlineStatus(true)
  })

  afterEach(() => {
    // Restore original
    if (originalNavigatorOnline) {
      Object.defineProperty(navigator, 'onLine', originalNavigatorOnline)
    }
  })

  it('does not render banner when online', () => {
    render(<OfflineBanner />)
    expect(screen.queryByRole('status')).toBeNull()
  })

  it('renders banner when offline at mount', () => {
    setOnlineStatus(false)
    render(<OfflineBanner />)
    const banner = screen.getByRole('status')
    expect(banner).toBeDefined()
    expect(banner.textContent).toContain('You are offline')
  })

  it('shows banner when offline event fires', () => {
    render(<OfflineBanner />)
    expect(screen.queryByRole('status')).toBeNull()

    act(() => {
      window.dispatchEvent(new Event('offline'))
    })

    const banner = screen.getByRole('status')
    expect(banner).toBeDefined()
    expect(banner.textContent).toContain('Some features may be unavailable')
  })

  it('hides banner when online event fires after going offline', () => {
    setOnlineStatus(false)
    render(<OfflineBanner />)
    expect(screen.getByRole('status')).toBeDefined()

    act(() => {
      setOnlineStatus(true)
      window.dispatchEvent(new Event('online'))
    })

    expect(screen.queryByRole('status')).toBeNull()
  })

  it('cleans up event listeners on unmount', () => {
    const removeEventListenerSpy = vi.spyOn(window, 'removeEventListener')
    const { unmount } = render(<OfflineBanner />)
    unmount()
    expect(removeEventListenerSpy).toHaveBeenCalledWith('offline', expect.any(Function))
    expect(removeEventListenerSpy).toHaveBeenCalledWith('online', expect.any(Function))
    removeEventListenerSpy.mockRestore()
  })
})
