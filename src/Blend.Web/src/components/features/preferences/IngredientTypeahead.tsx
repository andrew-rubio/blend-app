'use client'

import { useState, useRef, useCallback, type KeyboardEvent } from 'react'
import { Input } from '@/components/ui/Input'

export interface IngredientTypeaheadProps {
  addedIds: string[]
  onAdd: (id: string) => void
  onRemove: (id: string) => void
  disabled?: boolean
}

/**
 * Typeahead input that lets the user type ingredient names, press Enter to add
 * them, and displays added items as removable chips (PREF-11, PREF-12).
 *
 * Since there is no dedicated ingredient-search endpoint on the backend, items
 * are stored using the lowercase ingredient name as the ID.
 */
export function IngredientTypeahead({
  addedIds,
  onAdd,
  onRemove,
  disabled = false,
}: IngredientTypeaheadProps) {
  const [inputValue, setInputValue] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  const handleAdd = useCallback(() => {
    const trimmed = inputValue.trim().toLowerCase()
    if (!trimmed || addedIds.includes(trimmed)) {
      setInputValue('')
      return
    }
    onAdd(trimmed)
    setInputValue('')
  }, [inputValue, addedIds, onAdd])

  const handleKeyDown = useCallback(
    (e: KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter') {
        e.preventDefault()
        handleAdd()
      }
    },
    [handleAdd]
  )

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-2">
        <div className="flex-1">
          <Input
            ref={inputRef}
            id="ingredient-search"
            placeholder="Type an ingredient name and press Enter"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            disabled={disabled}
            aria-label="Add disliked ingredient"
            autoComplete="off"
          />
        </div>
        <button
          type="button"
          onClick={handleAdd}
          disabled={disabled || !inputValue.trim()}
          aria-label="Add ingredient"
          className="inline-flex items-center justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-300 dark:hover:bg-gray-800"
        >
          Add
        </button>
      </div>

      {addedIds.length > 0 && (
        <ul className="flex flex-wrap gap-2" aria-label="Added disliked ingredients">
          {addedIds.map((id) => (
            <li key={id}>
              <span className="inline-flex items-center gap-1.5 rounded-full border border-gray-200 bg-gray-100 px-3 py-1 text-sm text-gray-800 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200">
                {id}
                <button
                  type="button"
                  onClick={() => onRemove(id)}
                  disabled={disabled}
                  aria-label={`Remove ${id}`}
                  className="ml-0.5 inline-flex h-4 w-4 items-center justify-center rounded-full text-gray-500 hover:bg-gray-200 hover:text-gray-700 focus:outline-none focus:ring-1 focus:ring-primary-500 disabled:cursor-not-allowed dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-gray-200"
                >
                  <svg viewBox="0 0 16 16" fill="currentColor" className="h-3 w-3" aria-hidden="true">
                    <path d="M4.646 4.646a.5.5 0 01.708 0L8 7.293l2.646-2.647a.5.5 0 01.708.708L8.707 8l2.647 2.646a.5.5 0 01-.708.708L8 8.707l-2.646 2.647a.5.5 0 01-.708-.708L7.293 8 4.646 5.354a.5.5 0 010-.708z" />
                  </svg>
                </button>
              </span>
            </li>
          ))}
        </ul>
      )}

      {addedIds.length === 0 && (
        <p className="text-sm text-gray-500 dark:text-gray-400">
          No ingredients added yet. Type an ingredient name above and press Enter or click Add.
        </p>
      )}
    </div>
  )
}
