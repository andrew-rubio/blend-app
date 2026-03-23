'use client'

import { useRef, useEffect } from 'react'
import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export interface SearchInputProps {
  value: string
  onChange: (value: string) => void
  onClear: () => void
  placeholder?: string
  isLoading?: boolean
  className?: string
}

/**
 * Search input with a leading search icon, clear button, and loading indicator.
 * The debouncing is handled by the parent (ExploreContainer) using a 300ms delay (EXPL-08).
 */
export function SearchInput({
  value,
  onChange,
  onClear,
  placeholder = 'Search recipes…',
  isLoading = false,
  className,
}: SearchInputProps) {
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    inputRef.current?.focus()
  }, [])

  return (
    <div
      className={twMerge(
        clsx(
          'relative flex items-center rounded-xl border bg-white shadow-sm',
          'border-gray-300 dark:border-gray-600 dark:bg-gray-900',
          'focus-within:border-primary-500 focus-within:ring-2 focus-within:ring-primary-500 focus-within:ring-offset-0',
          className
        )
      )}
    >
      {/* Search icon / spinner */}
      <span className="pointer-events-none absolute left-3 flex items-center text-gray-400" aria-hidden="true">
        {isLoading ? (
          <span className="h-5 w-5 animate-spin rounded-full border-2 border-primary-500 border-t-transparent" />
        ) : (
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        )}
      </span>

      <input
        ref={inputRef}
        type="search"
        role="searchbox"
        aria-label="Search recipes"
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={clsx(
          'block w-full rounded-xl bg-transparent py-3 pl-10 pr-10 text-sm',
          'text-gray-900 placeholder:text-gray-400 dark:text-gray-100',
          'focus:outline-none'
        )}
      />

      {/* Clear button */}
      {value && (
        <button
          type="button"
          aria-label="Clear search"
          onClick={onClear}
          className="absolute right-3 flex items-center text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      )}
    </div>
  )
}
