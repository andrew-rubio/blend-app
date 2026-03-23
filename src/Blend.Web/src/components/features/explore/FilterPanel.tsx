'use client'

import { clsx } from 'clsx'
import { useSearchStore, selectActiveFilterCount } from '@/stores/searchStore'
import { useCuisines, useDishTypes, useDiets } from '@/hooks/usePreferences'
import type { SearchFilters } from '@/types'

const MAX_READY_TIME_OPTIONS = [15, 30, 45, 60] as const

interface ChipProps {
  label: string
  selected: boolean
  onClick: () => void
}

function FilterChip({ label, selected, onClick }: ChipProps) {
  return (
    <button
      type="button"
      role="checkbox"
      aria-checked={selected}
      aria-label={label}
      onClick={onClick}
      className={clsx(
        'inline-flex items-center rounded-full border px-3 py-1 text-sm font-medium transition-colors',
        'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
        selected
          ? 'border-primary-600 bg-primary-600 text-white hover:bg-primary-700'
          : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-300 dark:hover:bg-gray-800'
      )}
    >
      {label}
    </button>
  )
}

interface FilterSectionProps {
  title: string
  items: string[]
  selected: string[]
  onToggle: (item: string) => void
}

function FilterSection({ title, items, selected, onToggle }: FilterSectionProps) {
  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300">{title}</h3>
      <div className="flex flex-wrap gap-2" role="group" aria-label={title}>
        {items.map((item) => (
          <FilterChip
            key={item}
            label={item}
            selected={selected.includes(item)}
            onClick={() => onToggle(item)}
          />
        ))}
      </div>
    </div>
  )
}

export interface FilterPanelProps {
  onClose: () => void
}

/**
 * Slide-in filter panel (EXPL-10 through EXPL-12).
 * Provides chips for cuisines, diets, dish types, and max ready time.
 */
export function FilterPanel({ onClose }: FilterPanelProps) {
  const { filters, toggleCuisineFilter, toggleDietFilter, toggleDishTypeFilter, setMaxReadyTime, clearFilters } =
    useSearchStore()

  const { data: cuisines = [] } = useCuisines()
  const { data: dishTypes = [] } = useDishTypes()
  const { data: diets = [] } = useDiets()

  const activeCount = selectActiveFilterCount(filters)

  function handleClearAll() {
    clearFilters()
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Filter recipes"
      className="flex h-full flex-col overflow-y-auto bg-white dark:bg-gray-900"
    >
      {/* Header */}
      <div className="flex items-center justify-between border-b border-gray-200 px-4 py-3 dark:border-gray-700">
        <h2 className="text-base font-semibold text-gray-900 dark:text-white">
          Filters
          {activeCount > 0 && (
            <span className="ml-2 inline-flex h-5 w-5 items-center justify-center rounded-full bg-primary-600 text-xs text-white">
              {activeCount}
            </span>
          )}
        </h2>
        <button
          type="button"
          aria-label="Close filter panel"
          onClick={onClose}
          className="rounded-md p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      {/* Filter sections */}
      <div className="flex flex-1 flex-col gap-6 overflow-y-auto p-4">
        {cuisines.length > 0 && (
          <FilterSection
            title="Cuisines"
            items={cuisines}
            selected={filters.cuisines}
            onToggle={toggleCuisineFilter}
          />
        )}

        {diets.length > 0 && (
          <FilterSection
            title="Diets"
            items={diets}
            selected={filters.diets}
            onToggle={toggleDietFilter}
          />
        )}

        {dishTypes.length > 0 && (
          <FilterSection
            title="Dish Types"
            items={dishTypes}
            selected={filters.dishTypes}
            onToggle={toggleDishTypeFilter}
          />
        )}

        {/* Max ready time */}
        <div className="flex flex-col gap-2">
          <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300">Max Ready Time</h3>
          <div className="flex flex-wrap gap-2" role="group" aria-label="Max ready time">
            {MAX_READY_TIME_OPTIONS.map((mins) => (
              <FilterChip
                key={mins}
                label={`${mins} min`}
                selected={filters.maxReadyTime === mins}
                onClick={() =>
                  setMaxReadyTime(filters.maxReadyTime === mins ? null : mins)
                }
              />
            ))}
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="border-t border-gray-200 px-4 py-3 dark:border-gray-700">
        <button
          type="button"
          onClick={handleClearAll}
          disabled={activeCount === 0}
          aria-label="Clear all filters"
          className={clsx(
            'w-full rounded-lg border px-4 py-2 text-sm font-medium transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
            activeCount === 0
              ? 'cursor-not-allowed border-gray-200 text-gray-400 dark:border-gray-700 dark:text-gray-600'
              : 'border-red-300 text-red-600 hover:bg-red-50 dark:border-red-700 dark:text-red-400 dark:hover:bg-red-900/20'
          )}
        >
          Clear all filters
        </button>
      </div>
    </div>
  )
}

/** Floating filter button with active-count badge (EXPL-12). */
export function FilterButton({
  filters,
  onClick,
}: {
  filters: SearchFilters
  onClick: () => void
}) {
  const activeCount = selectActiveFilterCount(filters)

  return (
    <button
      type="button"
      aria-label={`Filter recipes${activeCount > 0 ? `, ${activeCount} active` : ''}`}
      onClick={onClick}
      className={clsx(
        'relative flex items-center gap-1.5 rounded-xl border px-3 py-2.5 text-sm font-medium transition-colors',
        'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
        activeCount > 0
          ? 'border-primary-600 bg-primary-50 text-primary-700 dark:bg-primary-900/30 dark:text-primary-300'
          : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-300 dark:hover:bg-gray-800'
      )}
    >
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2a1 1 0 01-.293.707L13 13.414V19a1 1 0 01-.553.894l-4 2A1 1 0 017 21v-7.586L3.293 6.707A1 1 0 013 6V4z" />
      </svg>
      Filters
      {activeCount > 0 && (
        <span
          aria-hidden="true"
          className="flex h-5 w-5 items-center justify-center rounded-full bg-primary-600 text-xs font-bold text-white"
        >
          {activeCount}
        </span>
      )}
    </button>
  )
}
