'use client'

import type { Recipe } from '@/types'

interface DirectionsTabProps {
  recipe: Recipe
}

export function DirectionsTab({ recipe }: DirectionsTabProps) {
  if (recipe.steps.length === 0) {
    return (
      <p className="text-sm text-gray-500 dark:text-gray-400">No directions available.</p>
    )
  }

  return (
    <ol className="flex flex-col gap-6" aria-label="Directions">
      {recipe.steps.map((step) => (
        <li key={step.number} className="flex gap-4">
          {/* Step number bubble */}
          <div
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary-600 text-sm font-bold text-white"
            aria-hidden="true"
          >
            {step.number}
          </div>

          <div className="flex flex-col gap-3 pt-0.5">
            <p className="text-base leading-relaxed text-gray-700 dark:text-gray-300">
              {step.step}
            </p>

            {step.imageUrl && (
              <div className="overflow-hidden rounded-lg bg-gray-100 dark:bg-gray-800 max-w-sm">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={step.imageUrl}
                  alt={`Step ${step.number}`}
                  className="h-48 w-full object-cover"
                />
              </div>
            )}
          </div>
        </li>
      ))}
    </ol>
  )
}
