'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/Button'
import type { Recipe, RecipeIngredient } from '@/types'

interface IngredientsTabProps {
  recipe: Recipe
}

/** Scales an ingredient amount by the ratio of adjusted servings to base servings. */
function scaleAmount(amount: number, base: number, adjusted: number): number {
  if (base === 0) return amount
  const scaled = (amount * adjusted) / base
  // Round to at most 2 decimal places, removing trailing zeros
  return Math.round(scaled * 100) / 100
}

export function IngredientsTab({ recipe }: IngredientsTabProps) {
  const [servings, setServings] = useState(recipe.servings)

  const decrement = () => setServings((s) => Math.max(1, s - 1))
  const increment = () => setServings((s) => s + 1)

  return (
    <div className="flex flex-col gap-6">
      {/* Serving adjuster */}
      <div
        className="flex items-center gap-4 rounded-xl border border-gray-200 p-4 dark:border-gray-700"
        aria-label="Serving adjuster"
      >
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Servings</span>
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={decrement}
            aria-label="Decrease servings"
            disabled={servings <= 1}
          >
            −
          </Button>
          <span
            className="w-8 text-center text-lg font-semibold text-gray-900 dark:text-white"
            aria-live="polite"
            aria-label={`${servings} servings`}
          >
            {servings}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={increment}
            aria-label="Increase servings"
          >
            +
          </Button>
        </div>
      </div>

      {/* Ingredient list */}
      <ul className="divide-y divide-gray-100 dark:divide-gray-800" aria-label="Ingredients">
        {recipe.ingredients.map((ingredient) => (
          <IngredientRow
            key={ingredient.id}
            ingredient={ingredient}
            baseServings={recipe.servings}
            adjustedServings={servings}
          />
        ))}
      </ul>
    </div>
  )
}

interface IngredientRowProps {
  ingredient: RecipeIngredient
  baseServings: number
  adjustedServings: number
}

function IngredientRow({ ingredient, baseServings, adjustedServings }: IngredientRowProps) {
  const scaledAmount = scaleAmount(ingredient.amount, baseServings, adjustedServings)

  return (
    <li className="flex items-center gap-3 py-3">
      <span className="w-24 shrink-0 text-right text-sm font-semibold text-gray-900 dark:text-white">
        {scaledAmount} {ingredient.unit}
      </span>
      <span className="text-sm text-gray-700 dark:text-gray-300 capitalize">{ingredient.name}</span>
    </li>
  )
}
