'use client'

import { useState, useEffect, useRef, useCallback } from 'react'
import { useIngredientSearch } from '@/hooks/useCookMode'
import type { IngredientSearchResult } from '@/types'

export interface IngredientSearchProps {
  onAdd: (ingredient: IngredientSearchResult) => void
  disabled?: boolean
}

const DEBOUNCE_MS = 200

export function IngredientSearch({ onAdd, disabled }: IngredientSearchProps) {
  const [inputValue, setInputValue] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(-1)
  const listRef = useRef<HTMLUListElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedQuery(inputValue)
      setActiveIndex(-1)
    }, DEBOUNCE_MS)
    return () => clearTimeout(id)
  }, [inputValue])

  const { data: results = [] } = useIngredientSearch(debouncedQuery)

  useEffect(() => {
    setIsOpen(debouncedQuery.length >= 2)
  }, [debouncedQuery, results])

  const handleSelect = useCallback(
    (ingredient: IngredientSearchResult) => {
      onAdd(ingredient)
      setInputValue('')
      setDebouncedQuery('')
      setIsOpen(false)
      setActiveIndex(-1)
    },
    [onAdd]
  )

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (!isOpen) return
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActiveIndex((i) => Math.min(i + 1, results.length - 1))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiveIndex((i) => Math.max(i - 1, 0))
    } else if (e.key === 'Enter') {
      e.preventDefault()
      if (activeIndex >= 0 && results[activeIndex]) {
        handleSelect(results[activeIndex])
      }
    } else if (e.key === 'Escape') {
      setIsOpen(false)
      setActiveIndex(-1)
    }
  }

  const listboxId = 'ingredient-search-listbox'

  return (
    <div className="relative" data-testid="ingredient-search">
      <div className="relative flex items-center">
        <span className="absolute left-3 text-gray-400" aria-hidden="true">
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </span>
        <input
          ref={inputRef}
          type="text"
          role="combobox"
          aria-expanded={isOpen}
          aria-controls={listboxId}
          aria-autocomplete="list"
          aria-activedescendant={activeIndex >= 0 ? `ingredient-option-${activeIndex}` : undefined}
          placeholder="Search ingredients…"
          value={inputValue}
          disabled={disabled}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={handleKeyDown}
          onFocus={() => debouncedQuery.length >= 2 && setIsOpen(true)}
          onBlur={() => setTimeout(() => setIsOpen(false), 150)}
          className="block w-full rounded-md border border-gray-300 bg-white py-2 pl-10 pr-3 text-sm placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
          data-testid="ingredient-search-input"
        />
      </div>
      {isOpen && (
        <ul
          id={listboxId}
          ref={listRef}
          role="listbox"
          aria-label="Ingredient suggestions"
          className="absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-md border border-gray-200 bg-white py-1 shadow-lg dark:border-gray-700 dark:bg-gray-800"
          data-testid="ingredient-search-dropdown"
        >
          {results.length === 0 ? (
            <li className="px-4 py-2 text-sm text-gray-500 dark:text-gray-400" data-testid="ingredient-search-empty">
              No ingredients found
            </li>
          ) : (
            results.map((ingredient, index) => (
              <li
                key={ingredient.id}
                id={`ingredient-option-${index}`}
                role="option"
                aria-selected={index === activeIndex}
                className={`cursor-pointer px-4 py-2 text-sm ${
                  index === activeIndex
                    ? 'bg-primary-50 text-primary-700 dark:bg-primary-900 dark:text-primary-300'
                    : 'text-gray-900 hover:bg-gray-100 dark:text-gray-100 dark:hover:bg-gray-700'
                }`}
                onMouseDown={() => handleSelect(ingredient)}
                data-testid={`ingredient-option-${index}`}
              >
                <span className="font-medium">{ingredient.name}</span>
                {ingredient.category && (
                  <span className="ml-2 text-gray-400 dark:text-gray-500">{ingredient.category}</span>
                )}
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  )
}
