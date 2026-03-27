'use client'

import { useEffect, useState } from 'react'

/**
 * Displays a banner when the user's browser loses network connectivity.
 * Disappears automatically when connectivity is restored.
 */
export function OfflineBanner() {
  const [isOffline, setIsOffline] = useState(false)

  useEffect(() => {
    // Set initial state based on browser navigator
    setIsOffline(!navigator.onLine)

    const handleOffline = () => setIsOffline(true)
    const handleOnline = () => setIsOffline(false)

    window.addEventListener('offline', handleOffline)
    window.addEventListener('online', handleOnline)

    return () => {
      window.removeEventListener('offline', handleOffline)
      window.removeEventListener('online', handleOnline)
    }
  }, [])

  if (!isOffline) {
    return null
  }

  return (
    <div
      role="status"
      aria-live="polite"
      className="fixed inset-x-0 top-0 z-50 flex items-center justify-center bg-yellow-400 px-4 py-2 text-sm font-medium text-yellow-900 dark:bg-yellow-700 dark:text-yellow-100"
    >
      You are offline. Some features may be unavailable.
    </div>
  )
}
