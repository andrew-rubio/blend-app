'use client'

import { useState, useEffect, useCallback } from 'react'
import { useRouter, useSearchParams, usePathname } from 'next/navigation'
import { SearchInput } from './SearchInput'
import { FilterButton, FilterPanel } from './FilterPanel'
import { ExploreLanding } from './ExploreLanding'
import { SearchResults } from './SearchResults'
import { useSearchStore, selectActiveFilterCount } from '@/stores/searchStore'

const SEARCH_DEBOUNCE_MS = 300

/**
 * Top-level Explore page container.
 * Manages debounced search input, URL sync, and filter panel visibility (EXPL-07 through EXPL-36).
 */
export function ExploreContainer() {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  // ── Zustand store ──────────────────────────────────────────────────────────
  const { query, filters, isFilterPanelOpen, setQuery, openFilterPanel, closeFilterPanel } =
    useSearchStore()

  // ── Local debounced query (for API calls) ──────────────────────────────────
  const [inputValue, setInputValue] = useState<string>(() => searchParams.get('q') ?? query)
  const [debouncedQuery, setDebouncedQuery] = useState<string>(inputValue)

  // Sync URL → store on initial mount
  useEffect(() => {
    const q = searchParams.get('q') ?? ''
    setQuery(q)
    setInputValue(q)
    setDebouncedQuery(q)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // Debounce input → debouncedQuery (300ms) (EXPL-08)
  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedQuery(inputValue)
      setQuery(inputValue)
      updateUrl(inputValue)
    }, SEARCH_DEBOUNCE_MS)
    return () => clearTimeout(id)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [inputValue])

  // ── URL sync ───────────────────────────────────────────────────────────────
  const updateUrl = useCallback(
    (q: string) => {
      const params = new URLSearchParams(searchParams.toString())
      if (q) {
        params.set('q', q)
      } else {
        params.delete('q')
      }
      const newUrl = params.toString() ? `${pathname}?${params.toString()}` : pathname
      router.replace(newUrl, { scroll: false })
    },
    [router, pathname, searchParams]
  )

  // ── Handlers ───────────────────────────────────────────────────────────────
  function handleInputChange(value: string) {
    setInputValue(value)
  }

  function handleClear() {
    setInputValue('')
    setDebouncedQuery('')
    setQuery('')
    updateUrl('')
  }

  const isSearchActive = debouncedQuery.trim().length > 0 || selectActiveFilterCount(filters) > 0

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-6">
      {/* Page header */}
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Explore</h1>

      {/* Search row */}
      <div className="flex items-center gap-3">
        <SearchInput
          value={inputValue}
          onChange={handleInputChange}
          onClear={handleClear}
          className="flex-1"
        />
        <FilterButton filters={filters} onClick={openFilterPanel} />
      </div>

      {/* Filter panel overlay */}
      {isFilterPanelOpen && (
        <>
          {/* Backdrop */}
          <div
            className="fixed inset-0 z-40 bg-black/30 dark:bg-black/50"
            aria-hidden="true"
            onClick={closeFilterPanel}
          />
          {/* Panel */}
          <aside
            className="fixed inset-y-0 right-0 z-50 w-80 max-w-full shadow-xl"
            aria-label="Filter panel"
          >
            <FilterPanel onClose={closeFilterPanel} />
          </aside>
        </>
      )}

      {/* Main content */}
      {isSearchActive ? (
        <SearchResults query={debouncedQuery} filters={filters} />
      ) : (
        <ExploreLanding />
      )}
    </div>
  )
}
