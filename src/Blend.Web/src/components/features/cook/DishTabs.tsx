'use client'

import type { CookingSessionDish } from '@/types'

export interface DishTabsProps {
  dishes: CookingSessionDish[]
  activeDishId: string | null
  onSelect: (dishId: string) => void
  onAdd: () => void
  onRemove: (dishId: string) => void
  onRename: (dishId: string, name: string) => void
}

export function DishTabs({ dishes, activeDishId, onSelect, onAdd, onRemove }: DishTabsProps) {
  function handleRemove(dish: CookingSessionDish) {
    if (dishes.length <= 1) return
    if (window.confirm(`Remove dish "${dish.name}"?`)) {
      onRemove(dish.dishId)
    }
  }

  return (
    <div className="flex items-center gap-1 overflow-x-auto border-b border-gray-200 dark:border-gray-700" data-testid="dish-tabs">
      {dishes.map((dish) => (
        <div
          key={dish.dishId}
          className={`group flex items-center gap-1 border-b-2 px-3 py-2 ${
            activeDishId === dish.dishId
              ? 'border-primary-600 text-primary-600 dark:border-primary-400 dark:text-primary-400'
              : 'border-transparent text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-200'
          }`}
          data-testid={`dish-tab-${dish.dishId}`}
        >
          <button
            type="button"
            onClick={() => onSelect(dish.dishId)}
            className="whitespace-nowrap text-sm font-medium"
            aria-label={`Select dish ${dish.name}`}
            data-testid={`dish-tab-select-${dish.dishId}`}
          >
            {dish.name}
          </button>
          <button
            type="button"
            onClick={() => handleRemove(dish)}
            disabled={dishes.length <= 1}
            aria-label={`Remove dish ${dish.name}`}
            className="rounded p-0.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 disabled:cursor-not-allowed disabled:opacity-30 dark:hover:bg-gray-700"
            data-testid={`dish-tab-remove-${dish.dishId}`}
          >
            ×
          </button>
        </div>
      ))}
      <button
        type="button"
        onClick={onAdd}
        aria-label="Add dish"
        className="flex-shrink-0 rounded p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700"
        data-testid="dish-tab-add"
      >
        +
      </button>
    </div>
  )
}
