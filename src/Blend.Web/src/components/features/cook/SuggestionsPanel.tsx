'use client'

import { useState, useMemo } from 'react'
import { useSuggestions } from '@/hooks/useCookMode'
import type { SmartSuggestion } from '@/types'

export interface SuggestionsPanelProps {
  sessionId: string
  dishId?: string
  onAdd: (suggestion: SmartSuggestion) => void
}

export function SuggestionsPanel({ sessionId, dishId, onAdd }: SuggestionsPanelProps) {
  const { data, isLoading } = useSuggestions(sessionId, dishId)
  const [activeCategory, setActiveCategory] = useState<string | null>(null)

  const suggestions = useMemo(() => data?.suggestions ?? [], [data?.suggestions])

  const categories = useMemo(() => {
    const cats = new Set<string>()
    for (const s of suggestions) {
      if (s.category) cats.add(s.category)
    }
    return Array.from(cats).sort()
  }, [suggestions])

  const filtered = activeCategory
    ? suggestions.filter((s) => s.category === activeCategory)
    : suggestions

  if (isLoading) {
    return (
      <div className="flex flex-col gap-3 p-4" data-testid="suggestions-loading">
        {[0, 1, 2].map((i) => (
          <div key={i} className="animate-pulse rounded-md bg-gray-200 p-3 dark:bg-gray-700" data-testid={`suggestion-skeleton-${i}`}>
            <div className="mb-2 h-4 w-3/4 rounded bg-gray-300 dark:bg-gray-600" />
            <div className="h-2 w-full rounded bg-gray-300 dark:bg-gray-600" />
          </div>
        ))}
      </div>
    )
  }

  if (data?.kbUnavailable) {
    return (
      <div className="p-4" data-testid="suggestions-unavailable">
        <p className="text-sm text-gray-500 dark:text-gray-400">Ingredient suggestions are unavailable</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2 p-4" data-testid="suggestions-panel">
      {categories.length > 1 && (
        <div className="flex flex-wrap gap-1 pb-1" data-testid="suggestion-filters">
          <button
            type="button"
            onClick={() => setActiveCategory(null)}
            className={`rounded-full px-2.5 py-0.5 text-xs font-medium transition-colors ${
              activeCategory === null
                ? 'bg-primary-100 text-primary-700 dark:bg-primary-900 dark:text-primary-300'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600'
            }`}
            data-testid="suggestion-filter-all"
          >
            All
          </button>
          {categories.map((cat) => (
            <button
              key={cat}
              type="button"
              onClick={() => setActiveCategory(cat)}
              className={`rounded-full px-2.5 py-0.5 text-xs font-medium transition-colors ${
                activeCategory === cat
                  ? 'bg-primary-100 text-primary-700 dark:bg-primary-900 dark:text-primary-300'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600'
              }`}
              data-testid={`suggestion-filter-${cat}`}
            >
              {cat}
            </button>
          ))}
        </div>
      )}

      {filtered.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400" data-testid="suggestions-empty">No suggestions available</p>
      ) : (
        filtered.map((suggestion) => (
          <button
            key={suggestion.ingredientId}
            type="button"
            onClick={() => onAdd(suggestion)}
            className="w-full rounded-md border border-gray-200 bg-white p-3 text-left shadow-sm hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:hover:bg-gray-700"
            data-testid={`suggestion-${suggestion.ingredientId}`}
          >
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-900 dark:text-white">{suggestion.name}</span>
              <span className="text-xs text-gray-500 dark:text-gray-400">{Math.round(suggestion.aggregateScore * 100)}%</span>
            </div>
            <div className="mt-1 h-1.5 w-full overflow-hidden rounded-full bg-gray-200 dark:bg-gray-700">
              <div
                className="h-full rounded-full bg-primary-500"
                style={{ width: `${Math.min(suggestion.aggregateScore * 100, 100)}%` }}
                role="progressbar"
                aria-valuenow={Math.round(suggestion.aggregateScore * 100)}
                aria-valuemin={0}
                aria-valuemax={100}
                aria-label={`Match score: ${Math.round(suggestion.aggregateScore * 100)}%`}
              />
            </div>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{suggestion.reason}</p>
          </button>
        ))
      )}
    </div>
  )
}
