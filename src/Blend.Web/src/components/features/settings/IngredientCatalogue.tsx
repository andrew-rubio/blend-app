'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import { useIngredientSearch, useIngredientCatalogue } from '@/hooks/useIngredientSubmissions'
import type { CatalogueIngredient } from '@/types'

interface IngredientDetailViewProps {
  ingredient: CatalogueIngredient
  onBack: () => void
}

function IngredientDetailView({ ingredient, onBack }: IngredientDetailViewProps) {
  return (
    <div>
      <button
        type="button"
        onClick={onBack}
        className="mb-4 flex items-center gap-1 text-sm text-primary-600 hover:underline focus:outline-none focus:ring-2 focus:ring-primary-500"
        aria-label="Back to ingredient list"
      >
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
        Back
      </button>
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900">
        <h3 className="mb-1 text-lg font-semibold text-gray-900 dark:text-white">{ingredient.name}</h3>
        {ingredient.category && (
          <p className="mb-2 text-sm text-gray-500 dark:text-gray-400">
            <span className="font-medium">Category:</span> {ingredient.category}
          </p>
        )}
        {ingredient.flavourProfile && (
          <p className="text-sm text-gray-500 dark:text-gray-400">
            <span className="font-medium">Flavour profile:</span> {ingredient.flavourProfile}
          </p>
        )}
      </div>
    </div>
  )
}

/**
 * Browsable ingredient catalogue with search (SETT-04 through SETT-07).
 */
export function IngredientCatalogue() {
  const [searchQuery, setSearchQuery] = useState('')
  const [selected, setSelected] = useState<CatalogueIngredient | null>(null)

  const { data: searchResults = [], isFetching: isSearching } = useIngredientSearch(searchQuery)
  const { data: catalogue, isLoading: isCatalogueLoading } = useIngredientCatalogue()

  const isSearchMode = searchQuery.trim().length > 0
  const displayList: CatalogueIngredient[] = isSearchMode
    ? searchResults
    : (catalogue?.ingredients ?? [])

  if (selected) {
    return <IngredientDetailView ingredient={selected} onBack={() => setSelected(null)} />
  }

  return (
    <div>
      <div className="mb-4">
        <label htmlFor="ingredient-catalogue-search" className="sr-only">
          Search ingredients
        </label>
        <input
          id="ingredient-catalogue-search"
          type="search"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          placeholder="Search ingredients…"
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
          aria-label="Search ingredients"
        />
      </div>

      {(isCatalogueLoading || isSearching) && (
        <ul aria-busy="true" aria-label="Loading ingredients" className="space-y-2">
          {[1, 2, 3].map((i) => (
            <li key={i} className="animate-pulse rounded-lg bg-gray-100 py-4 dark:bg-gray-800" />
          ))}
        </ul>
      )}

      {!isCatalogueLoading && !isSearching && displayList.length === 0 && (
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {isSearchMode ? 'No ingredients found.' : 'No ingredients in catalogue.'}
        </p>
      )}

      {!isCatalogueLoading && !isSearching && displayList.length > 0 && (
        <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800" aria-label="Ingredient list">
          {displayList.map((ingredient) => (
            <li key={ingredient.id}>
              <button
                type="button"
                onClick={() => setSelected(ingredient)}
                className={clsx(
                  'flex w-full items-center justify-between px-4 py-3 text-left text-sm transition-colors',
                  'hover:bg-gray-50 dark:hover:bg-gray-800',
                  'focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500'
                )}
                aria-label={`View details for ${ingredient.name}`}
              >
                <div>
                  <span className="font-medium text-gray-900 dark:text-white">{ingredient.name}</span>
                  {ingredient.category && (
                    <span className="ml-2 text-xs text-gray-500 dark:text-gray-400">{ingredient.category}</span>
                  )}
                </div>
                <svg className="h-4 w-4 flex-shrink-0 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
