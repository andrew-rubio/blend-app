import { useEffect, useRef, useCallback } from 'react'

const ACTIVE_INTERVAL_MS = 15_000
const BACKGROUND_INTERVAL_MS = 120_000
const INACTIVITY_PAUSE_MS = 5 * 60_000

interface UseAdaptivePollingOptions {
  onPoll: () => void
  enabled?: boolean
}

/**
 * Adaptive polling hook per ADR 0010:
 * - 15s interval when app is active
 * - 120s interval when backgrounded
 * - Pauses after 5 minutes of user inactivity
 * - Resumes on user interaction
 */
export function useAdaptivePolling({ onPoll, enabled = true }: UseAdaptivePollingOptions) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const lastActivityRef = useRef<number>(Date.now())
  const isVisibleRef = useRef<boolean>(true)
  const onPollRef = useRef(onPoll)
  onPollRef.current = onPoll

  const clearTimer = useCallback(() => {
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current)
      timerRef.current = null
    }
  }, [])

  const scheduleNext = useCallback(() => {
    clearTimer()
    if (!enabled) return

    const now = Date.now()
    const inactive = now - lastActivityRef.current > INACTIVITY_PAUSE_MS
    if (inactive) return

    const interval = isVisibleRef.current ? ACTIVE_INTERVAL_MS : BACKGROUND_INTERVAL_MS
    timerRef.current = setTimeout(() => {
      onPollRef.current()
      scheduleNext()
    }, interval)
  }, [enabled, clearTimer])

  const recordActivity = useCallback(() => {
    const wasInactive = Date.now() - lastActivityRef.current > INACTIVITY_PAUSE_MS
    lastActivityRef.current = Date.now()
    if (wasInactive) {
      onPollRef.current()
      scheduleNext()
    }
  }, [scheduleNext])

  useEffect(() => {
    if (!enabled) {
      clearTimer()
      return
    }

    const handleVisibilityChange = () => {
      isVisibleRef.current = document.visibilityState === 'visible'
      if (isVisibleRef.current) {
        recordActivity()
      }
    }

    const handleActivity = () => {
      recordActivity()
    }

    document.addEventListener('visibilitychange', handleVisibilityChange)
    window.addEventListener('mousemove', handleActivity, { passive: true })
    window.addEventListener('keydown', handleActivity, { passive: true })
    window.addEventListener('click', handleActivity, { passive: true })
    window.addEventListener('touchstart', handleActivity, { passive: true })

    scheduleNext()

    return () => {
      clearTimer()
      document.removeEventListener('visibilitychange', handleVisibilityChange)
      window.removeEventListener('mousemove', handleActivity)
      window.removeEventListener('keydown', handleActivity)
      window.removeEventListener('click', handleActivity)
      window.removeEventListener('touchstart', handleActivity)
    }
  }, [enabled, scheduleNext, recordActivity, clearTimer])
}
