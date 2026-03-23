'use client'

import { clsx } from 'clsx'
import { useSearchStore } from '@/stores/searchStore'

/** Popular category shortcuts — activates a cuisine or dish-type filter (EXPL-05). */
const CATEGORY_SHORTCUTS = [
  { label: '🍝 Italian', type: 'cuisine', value: 'Italian' },
  { label: '🍣 Japanese', type: 'cuisine', value: 'Japanese' },
  { label: '🌮 Mexican', type: 'cuisine', value: 'Mexican' },
  { label: '🍛 Indian', type: 'cuisine', value: 'Indian' },
  { label: '🥗 Salad', type: 'dishType', value: 'salad' },
  { label: '🍰 Dessert', type: 'dishType', value: 'dessert' },
  { label: '🥣 Soup', type: 'dishType', value: 'soup' },
  { label: '🍳 Breakfast', type: 'dishType', value: 'breakfast' },
] as const

export interface CategoryShortcutsProps {
  /** Called when the user clicks a category chip, passing the active query string. */
  onSelect?: (query: string) => void
}

/**
 * Quick-filter chips for popular cuisines and dish types (EXPL-05).
 */
export function CategoryShortcuts({ onSelect }: CategoryShortcutsProps) {
  const { filters, toggleCuisineFilter, toggleDishTypeFilter } = useSearchStore()

  function handleClick(type: 'cuisine' | 'dishType', value: string) {
    if (type === 'cuisine') {
      toggleCuisineFilter(value)
    } else {
      toggleDishTypeFilter(value)
    }
    onSelect?.(value)
  }

  function isSelected(type: 'cuisine' | 'dishType', value: string) {
    return type === 'cuisine'
      ? filters.cuisines.includes(value)
      : filters.dishTypes.includes(value)
  }

  return (
    <section aria-labelledby="categories-heading">
      <h2
        id="categories-heading"
        className="mb-3 text-lg font-semibold text-gray-900 dark:text-white"
      >
        Browse by Category
      </h2>

      <div
        className="flex flex-wrap gap-2"
        role="group"
        aria-label="Category shortcuts"
      >
        {CATEGORY_SHORTCUTS.map(({ label, type, value }) => {
          const selected = isSelected(type, value)
          return (
            <button
              key={value}
              type="button"
              role="checkbox"
              aria-checked={selected}
              aria-label={label}
              onClick={() => handleClick(type, value)}
              className={clsx(
                'inline-flex items-center rounded-full border px-4 py-2 text-sm font-medium transition-colors',
                'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
                selected
                  ? 'border-primary-600 bg-primary-600 text-white hover:bg-primary-700'
                  : 'border-gray-200 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-300 dark:hover:bg-gray-800'
              )}
            >
              {label}
            </button>
          )
        })}
      </div>
    </section>
  )
}
