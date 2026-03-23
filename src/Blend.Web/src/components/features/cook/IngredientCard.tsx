'use client'

import type { SessionIngredient } from '@/types'

export interface IngredientCardProps {
  ingredient: SessionIngredient
  onRemove: (id: string) => void
  onDetail: (id: string) => void
  dishId?: string
}

export function IngredientCard({ ingredient, onRemove, onDetail, dishId: _dishId }: IngredientCardProps) {
  const initial = ingredient.name.charAt(0).toUpperCase()
  return (
    <div
      className="flex items-center gap-2 rounded-md border border-gray-200 bg-white p-2 shadow-sm dark:border-gray-700 dark:bg-gray-800"
      data-testid={`ingredient-card-${ingredient.ingredientId}`}
    >
      <button
        type="button"
        className="flex flex-1 items-center gap-2 text-left"
        onClick={() => onDetail(ingredient.ingredientId)}
        aria-label={`View details for ${ingredient.name}`}
        data-testid={`ingredient-card-body-${ingredient.ingredientId}`}
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-xs font-bold text-primary-700 dark:bg-primary-900 dark:text-primary-300">
          {initial}
        </span>
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium text-gray-900 dark:text-white">{ingredient.name}</p>
          {ingredient.notes && (
            <p className="truncate text-xs text-gray-500 dark:text-gray-400">{ingredient.notes}</p>
          )}
        </div>
      </button>
      <button
        type="button"
        onClick={() => onRemove(ingredient.ingredientId)}
        aria-label={`Remove ${ingredient.name}`}
        className="ml-auto flex-shrink-0 rounded p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
        data-testid={`ingredient-card-remove-${ingredient.ingredientId}`}
      >
        ×
      </button>
    </div>
  )
}
