'use client'

import { useState, useEffect, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { clsx } from 'clsx'

const DEFAULT_PLACEHOLDERS = [
  'Try searching for chicken...',
  'Find pasta recipes...',
  'Search for vegetarian dishes...',
  'Explore salmon dishes...',
  'Discover avocado recipes...',
]

export interface HomeSearchBarProps {
  initialPlaceholder?: string
  placeholders?: string[]
}

export function HomeSearchBar({ initialPlaceholder, placeholders = DEFAULT_PLACEHOLDERS }: HomeSearchBarProps) {
  const router = useRouter()
  const allPlaceholders = initialPlaceholder ? [initialPlaceholder, ...placeholders] : placeholders
  const [placeholderIndex, setPlaceholderIndex] = useState(0)

  useEffect(() => {
    const interval = setInterval(() => {
      setPlaceholderIndex((i) => (i + 1) % allPlaceholders.length)
    }, 3000)
    return () => clearInterval(interval)
  }, [allPlaceholders.length])

  const handleFocus = useCallback(() => {
    router.push('/explore?focus=search')
  }, [router])

  const handleClick = useCallback(() => {
    router.push('/explore?focus=search')
  }, [router])

  return (
    <div
      className="relative w-full"
      role="search"
      aria-label="Recipe search"
    >
      <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4">
        <svg
          className="h-5 w-5 text-gray-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
      </div>
      <input
        type="search"
        readOnly
        placeholder={allPlaceholders[placeholderIndex]}
        onClick={handleClick}
        onFocus={handleFocus}
        aria-label="Search for recipes"
        className={clsx(
          'block w-full cursor-pointer rounded-2xl border border-gray-200 bg-white py-3 pl-12 pr-4 text-sm shadow-sm',
          'placeholder:text-gray-400',
          'hover:border-gray-300 hover:shadow',
          'focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500',
          'dark:border-gray-700 dark:bg-gray-800 dark:placeholder:text-gray-500 dark:hover:border-gray-600',
          'transition-all duration-200'
        )}
      />
    </div>
  )
}
