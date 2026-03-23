'use client'

import { IngredientCard } from './IngredientCard'
import type { SessionIngredient } from '@/types'

export interface IngredientWorkspaceProps {
  ingredients: SessionIngredient[]
  onRemove: (id: string, dishId?: string) => void
  onDetail: (id: string) => void
  dishId?: string
}

export function IngredientWorkspace({ ingredients, onRemove, onDetail, dishId }: IngredientWorkspaceProps) {
  if (ingredients.length === 0) {
    return (
      <div className="flex items-center justify-center rounded-md border border-dashed border-gray-300 p-6 dark:border-gray-700" data-testid="ingredient-workspace-empty">
        <p className="text-sm text-gray-500 dark:text-gray-400">No ingredients added yet</p>
      </div>
    )
  }
  return (
    <div className="flex flex-col gap-2" data-testid="ingredient-workspace">
      {ingredients.map((ingredient) => (
        <IngredientCard
          key={ingredient.ingredientId}
          ingredient={ingredient}
          onRemove={(id) => onRemove(id, dishId)}
          onDetail={onDetail}
          dishId={dishId}
        />
      ))}
    </div>
  )
}
