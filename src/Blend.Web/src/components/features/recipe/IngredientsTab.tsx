'use client'

import { useState } from 'react'
import { ServingAdjuster } from './ServingAdjuster'
import type { Recipe } from '@/types/recipe'

interface IngredientsTabProps {
  recipe: Recipe
}

export function IngredientsTab({ recipe }: IngredientsTabProps) {
  const [servings, setServings] = useState(recipe.servings)

  const adjustedIngredients = recipe.ingredients.map((ing) => ({
    ...ing,
    amount: (ing.originalAmount / recipe.servings) * servings,
  }))

  return (
    <div className="space-y-4">
      <ServingAdjuster servings={servings} onServingsChange={setServings} />

      <ul className="divide-y divide-gray-100">
        {adjustedIngredients.map((ingredient) => (
          <li key={ingredient.id} className="flex items-center justify-between py-3">
            <span className="text-gray-800">{ingredient.name}</span>
            <span className="text-sm text-gray-500">
              {ingredient.amount % 1 === 0
                ? ingredient.amount.toString()
                : ingredient.amount.toFixed(2)}{' '}
              {ingredient.unit}
            </span>
          </li>
        ))}
      </ul>
    </div>
  )
}
