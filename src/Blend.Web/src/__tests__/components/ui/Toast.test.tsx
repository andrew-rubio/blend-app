import React from 'react'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import { ToastProvider, useToast } from '@/components/ui/Toast'

function ToastTrigger({
  message,
  variant,
  actionLabel,
  onAction,
}: {
  message?: string
  variant?: 'success' | 'error' | 'warning' | 'info'
  actionLabel?: string
  onAction?: () => void
}) {
  const { addToast } = useToast()
  return (
    <button
      onClick={() =>
        addToast(message ?? 'Hello toast', {
          variant,
          action: actionLabel ? { label: actionLabel, onClick: onAction ?? (() => {}) } : undefined,
        })
      }
    >
      Show toast
    </button>
  )
}

describe('ToastProvider and useToast', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('renders children without toasts initially', () => {
    render(
      <ToastProvider>
        <div data-testid="child">Content</div>
      </ToastProvider>
    )
    expect(screen.getByTestId('child')).toBeDefined()
  })

  it('shows a toast when addToast is called', () => {
    render(
      <ToastProvider>
        <ToastTrigger message="Test message" />
      </ToastProvider>
    )
    fireEvent.click(screen.getByText('Show toast'))
    expect(screen.getByText('Test message')).toBeDefined()
  })

  it('dismisses toast when dismiss button is clicked', () => {
    render(
      <ToastProvider>
        <ToastTrigger message="Dismiss me" />
      </ToastProvider>
    )
    fireEvent.click(screen.getByText('Show toast'))
    expect(screen.getByText('Dismiss me')).toBeDefined()

    const dismissBtn = screen.getByLabelText('Dismiss notification')
    fireEvent.click(dismissBtn)

    expect(screen.queryByText('Dismiss me')).toBeNull()
  })

  it('auto-dismisses toast after duration', () => {
    const { unmount } = render(
      <ToastProvider>
        <ToastTrigger message="Auto-dismiss" />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Show toast'))

    act(() => {
      vi.advanceTimersByTime(6000)
    })

    expect(screen.queryByText('Auto-dismiss')).toBeNull()
    unmount()
  })

  it('shows success variant with correct styling', () => {
    render(
      <ToastProvider>
        <ToastTrigger message="Success!" variant="success" />
      </ToastProvider>
    )
    fireEvent.click(screen.getByText('Show toast'))
    const alert = screen.getByRole('alert')
    expect(alert.className).toContain('green')
  })

  it('shows error variant with correct styling', () => {
    render(
      <ToastProvider>
        <ToastTrigger message="Error!" variant="error" />
      </ToastProvider>
    )
    fireEvent.click(screen.getByText('Show toast'))
    const alert = screen.getByRole('alert')
    expect(alert.className).toContain('red')
  })

  it('renders action button when action provided', () => {
    const onAction = vi.fn()
    render(
      <ToastProvider>
        <ToastTrigger message="With action" actionLabel="Undo" onAction={onAction} />
      </ToastProvider>
    )
    fireEvent.click(screen.getByText('Show toast'))
    expect(screen.getByText('Undo')).toBeDefined()

    fireEvent.click(screen.getByText('Undo'))
    expect(onAction).toHaveBeenCalledTimes(1)
  })

  it('throws when useToast is used outside provider', () => {
    const TestComponent = () => {
      useToast()
      return null
    }
    expect(() => render(<TestComponent />)).toThrow(
      'useToast must be used within a ToastProvider'
    )
  })
})
