'use client'

import { useState, useRef, useCallback, type ReactNode } from 'react'

export interface PullToRefreshProps {
  onRefresh: () => Promise<void>
  children: ReactNode
  threshold?: number
}

export function PullToRefresh({ onRefresh, children, threshold = 70 }: PullToRefreshProps) {
  const [pullDistance, setPullDistance] = useState(0)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const startY = useRef<number | null>(null)
  const containerRef = useRef<HTMLDivElement>(null)

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    const scrollTop = containerRef.current?.scrollTop ?? 0
    if (scrollTop === 0) {
      startY.current = e.touches[0].clientY
    }
  }, [])

  const handleTouchMove = useCallback(
    (e: React.TouchEvent) => {
      if (startY.current === null || isRefreshing) return
      const delta = e.touches[0].clientY - startY.current
      if (delta > 0) {
        setPullDistance(Math.min(delta * 0.5, threshold + 20))
      }
    },
    [isRefreshing, threshold]
  )

  const handleTouchEnd = useCallback(async () => {
    if (pullDistance >= threshold) {
      setIsRefreshing(true)
      setPullDistance(0)
      startY.current = null
      try {
        await onRefresh()
      } finally {
        setIsRefreshing(false)
      }
    } else {
      setPullDistance(0)
      startY.current = null
    }
  }, [onRefresh, pullDistance, threshold])

  const showIndicator = pullDistance > 10 || isRefreshing

  return (
    <div
      ref={containerRef}
      onTouchStart={handleTouchStart}
      onTouchMove={handleTouchMove}
      onTouchEnd={handleTouchEnd}
      className="relative"
    >
      {showIndicator && (
        <div
          className="absolute left-1/2 top-0 z-10 flex -translate-x-1/2 items-center justify-center transition-transform"
          style={{ transform: `translate(-50%, ${isRefreshing ? 8 : pullDistance - 24}px)` }}
          aria-live="polite"
          aria-label={isRefreshing ? 'Refreshing' : 'Pull to refresh'}
        >
          <div
            className={`flex h-8 w-8 items-center justify-center rounded-full bg-white shadow-md dark:bg-gray-800 ${isRefreshing ? 'animate-spin' : ''}`}
          >
            <svg
              className="h-4 w-4 text-orange-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
          </div>
        </div>
      )}
      <div style={{ transform: `translateY(${pullDistance}px)`, transition: pullDistance === 0 ? 'transform 0.2s ease' : 'none' }}>
        {children}
      </div>
    </div>
  )
}
