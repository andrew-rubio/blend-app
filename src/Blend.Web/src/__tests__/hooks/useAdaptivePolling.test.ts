import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useAdaptivePolling } from '@/hooks/useAdaptivePolling'

describe('useAdaptivePolling', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.restoreAllMocks()
  })

  it('calls onPoll after active interval (15s)', () => {
    const onPoll = vi.fn()
    renderHook(() => useAdaptivePolling({ onPoll }))
    act(() => {
      vi.advanceTimersByTime(15_000)
    })
    expect(onPoll).toHaveBeenCalledTimes(1)
  })

  it('calls onPoll multiple times as interval repeats', () => {
    const onPoll = vi.fn()
    renderHook(() => useAdaptivePolling({ onPoll }))
    act(() => {
      vi.advanceTimersByTime(45_000)
    })
    expect(onPoll).toHaveBeenCalledTimes(3)
  })

  it('does not schedule when enabled is false', () => {
    const onPoll = vi.fn()
    renderHook(() => useAdaptivePolling({ onPoll, enabled: false }))
    act(() => {
      vi.advanceTimersByTime(15_000)
    })
    expect(onPoll).not.toHaveBeenCalled()
  })

  it('pauses polling after inactivity period', () => {
    const onPoll = vi.fn()
    renderHook(() => useAdaptivePolling({ onPoll }))
    // Fast-forward past inactivity threshold
    act(() => {
      vi.advanceTimersByTime(5 * 60_000 + 15_000)
    })
    const callCount = onPoll.mock.calls.length
    // Further advance - should not trigger more polls
    act(() => {
      vi.advanceTimersByTime(15_000)
    })
    expect(onPoll.mock.calls.length).toBe(callCount)
  })

  it('cleans up timer on unmount', () => {
    const clearTimeoutSpy = vi.spyOn(global, 'clearTimeout')
    const onPoll = vi.fn()
    const { unmount } = renderHook(() => useAdaptivePolling({ onPoll }))
    unmount()
    expect(clearTimeoutSpy).toHaveBeenCalled()
  })
})
