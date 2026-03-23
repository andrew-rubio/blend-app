'use client'

import { useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { RecipeCard } from './RecipeCard'
import { SkeletonGrid } from './SkeletonCard'
import { Button } from '@/components/ui/Button'
import { useSearchRecipes } from '@/hooks/useSearch'
import type { SearchFilters, RecipeSearchResult } from '@/types'

export interface SearchResultsProps {
  query: string
  filters: SearchFilters
}

/**
 * Displays recipe search results in a responsive grid with "Load more" pagination (EXPL-13, EXPL-16).
 */
export function SearchResults({ query, filters }: SearchResultsProps) {
  const router = useRouter()

  const {
    data,
    isLoading,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    error,
  } = useSearchRecipes({ query, filters })

  const allRecipes: RecipeSearchResult[] =
    data?.pages.flatMap((page) => page.results) ?? []

  const totalResults = data?.pages[0]?.metadata.totalResults ?? 0
  const quotaExhausted = data?.pages[0]?.metadata.quotaExhausted ?? false

  const handleCardClick = useCallback(
    (id: string) => {
      router.push(`/recipes/${id}`)
    },
    [router]
  )

  if (isLoading) {
    return <SkeletonGrid count={8} />
  }

  if (error) {
    return (
      <div
        role="alert"
        className="flex flex-col items-center gap-3 py-16 text-center"
      >
        <p className="text-gray-500 dark:text-gray-400">
          Something went wrong. Please try again.
        </p>
        <Button variant="outline" size="sm" onClick={() => window.location.reload()}>
          Retry
        </Button>
      </div>
    )
  }

  if (allRecipes.length === 0) {
    return (
      <div
        role="status"
        aria-live="polite"
        className="flex flex-col items-center gap-3 py-16 text-center"
      >
        <svg
          className="h-16 w-16 text-gray-300 dark:text-gray-600"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={1}
          aria-hidden="true"
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <p className="text-lg font-medium text-gray-700 dark:text-gray-300">No results found</p>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Try broadening your search or removing some filters.
        </p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4">
      {/* Result count + quota notice */}
      <div
        className="flex flex-wrap items-center justify-between gap-2 text-sm text-gray-500 dark:text-gray-400"
        aria-live="polite"
      >
        <span>
          {totalResults.toLocaleString()} {totalResults === 1 ? 'result' : 'results'}
        </span>
        {quotaExhausted && (
          <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs text-amber-700 dark:bg-amber-900/30 dark:text-amber-300">
            Showing community results only
          </span>
        )}
      </div>

      {/* Grid */}
      <div
        className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4"
        role="list"
        aria-label="Search results"
      >
        {allRecipes.map((recipe) => (
          <div key={recipe.id} role="listitem">
            <RecipeCard recipe={recipe} onClick={handleCardClick} />
          </div>
        ))}
      </div>

      {/* Load more */}
      {hasNextPage && (
        <div className="flex justify-center pt-4">
          <Button
            variant="outline"
            isLoading={isFetchingNextPage}
            onClick={() => void fetchNextPage()}
            aria-label="Load more recipes"
          >
            {isFetchingNextPage ? 'Loading…' : 'Load more'}
          </Button>
        </div>
      )}

      {/* Fetching indicator */}
      {isFetchingNextPage && (
        <div
          className="flex justify-center pt-2"
          aria-live="polite"
          aria-label="Loading more recipes"
        >
          <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-500 border-t-transparent" />
        </div>
      )}
    </div>
  )
}
