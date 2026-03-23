'use client'

import type { CookingSession } from '@/types'

interface SessionSummaryStepProps {
  session: CookingSession
  onNext: () => void
}

/**
 * Step 1: Session Summary (COOK-30).
 * Displays all dishes, their ingredients, and notes from the completed session.
 */
export function SessionSummaryStep({ session, onNext }: SessionSummaryStepProps) {
  const totalIngredients =
    session.dishes.reduce((sum, d) => sum + d.ingredients.length, 0) +
    session.addedIngredients.length

  return (
    <div data-testid="session-summary-step">
      <h2 className="mb-1 text-xl font-semibold text-gray-900 dark:text-white">
        Session Summary
      </h2>
      <p className="mb-6 text-sm text-gray-500 dark:text-gray-400">
        Great cooking! Here's what you made today.
      </p>

      {/* Dishes */}
      {session.dishes.length > 0 && (
        <div className="mb-6 space-y-4">
          {session.dishes.map((dish) => (
            <div
              key={dish.dishId}
              className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800"
              data-testid={`dish-summary-${dish.dishId}`}
            >
              <h3 className="mb-2 font-medium text-gray-900 dark:text-white">{dish.name}</h3>
              {dish.ingredients.length > 0 ? (
                <ul className="space-y-1">
                  {dish.ingredients.map((ing) => (
                    <li
                      key={ing.ingredientId}
                      className="text-sm text-gray-600 dark:text-gray-300"
                    >
                      • {ing.name}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-sm text-gray-400 dark:text-gray-500">No ingredients added.</p>
              )}
              {dish.notes && (
                <p className="mt-2 text-sm italic text-gray-500 dark:text-gray-400">
                  Notes: {dish.notes}
                </p>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Session-level ingredients */}
      {session.addedIngredients.length > 0 && (
        <div className="mb-6 rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
          <h3 className="mb-2 font-medium text-gray-900 dark:text-white">Additional Ingredients</h3>
          <ul className="space-y-1">
            {session.addedIngredients.map((ing) => (
              <li key={ing.ingredientId} className="text-sm text-gray-600 dark:text-gray-300">
                • {ing.name}
              </li>
            ))}
          </ul>
        </div>
      )}

      {totalIngredients === 0 && (
        <p className="mb-6 text-sm text-gray-400 dark:text-gray-500">
          No ingredients were recorded in this session.
        </p>
      )}

      <div className="flex justify-end">
        <button
          type="button"
          onClick={onNext}
          className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Continue to pairing feedback"
          data-testid="summary-next-button"
        >
          Continue
        </button>
      </div>
    </div>
  )
}
