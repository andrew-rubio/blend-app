'use client'

import { useState } from 'react'
import type { Recipe } from '@/types/recipe'
import { ServingAdjuster } from './ServingAdjuster'

interface Props { recipe: Recipe }

export function IngredientsTab({ recipe }: Props) {
  const [servings, setServings] = useState(recipe.servings)
  const ratio = servings / recipe.servings

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold text-gray-900">Ingredients</h3>
        <ServingAdjuster servings={servings} onServingsChange={setServings} />
      </div>
      <ul className="divide-y divide-gray-100">
        {recipe.ingredients.map((ing) => {
          const scaledAmount = (ing.originalAmount * ratio).toFixed(
            Number.isInteger(ing.originalAmount * ratio) ? 0 : 1,
          )
          return (
            <li key={ing.id} className="flex justify-between py-3 text-sm">
              <span className="text-gray-900">{ing.name}</span>
              <span className="text-gray-500">
                {scaledAmount} {ing.unit}
              </span>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
