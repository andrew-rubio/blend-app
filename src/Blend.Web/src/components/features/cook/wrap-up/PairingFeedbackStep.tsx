'use client'

import type { CookingSession, PairingFeedbackItem, SessionIngredient } from '@/types'

interface PairingFeedbackStepProps {
  session: CookingSession
  feedbackItems: PairingFeedbackItem[]
  onRate: (ingredientId1: string, ingredientId2: string, rating: number, comment?: string) => void
  onNext: () => void
  onSkip: () => void
}

function StarRating({
  ingredientId1,
  ingredientId2,
  currentRating,
  onRate,
}: {
  ingredientId1: string
  ingredientId2: string
  currentRating: number
  onRate: (rating: number) => void
}) {
  return (
    <div
      className="flex gap-1"
      role="group"
      aria-label={`Rating for ${ingredientId1} and ${ingredientId2}`}
    >
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onRate(star)}
          aria-label={`${star} star${star !== 1 ? 's' : ''}`}
          aria-pressed={currentRating >= star}
          className={`text-xl focus:outline-none focus:ring-2 focus:ring-primary-500 ${
            currentRating >= star ? 'text-yellow-400' : 'text-gray-300 dark:text-gray-600'
          }`}
          data-testid={`star-${ingredientId1}-${ingredientId2}-${star}`}
        >
          ★
        </button>
      ))}
    </div>
  )
}

/** Builds all unique pairs from a list of ingredients. */
function buildPairs(ingredients: SessionIngredient[]): [SessionIngredient, SessionIngredient][] {
  const pairs: [SessionIngredient, SessionIngredient][] = []
  for (let i = 0; i < ingredients.length; i++) {
    for (let j = i + 1; j < ingredients.length; j++) {
      pairs.push([ingredients[i], ingredients[j]])
    }
  }
  return pairs
}

/**
 * Step 2: Ingredient Pairing Feedback (COOK-31 through COOK-35).
 * Shows star ratings for each ingredient pair, with an optional comment and skip option.
 */
export function PairingFeedbackStep({
  session,
  feedbackItems,
  onRate,
  onNext,
  onSkip,
}: PairingFeedbackStepProps) {
  // Collect pairs across all dishes
  const allPairs: [SessionIngredient, SessionIngredient][] = []
  for (const dish of session.dishes) {
    allPairs.push(...buildPairs(dish.ingredients))
  }
  allPairs.push(...buildPairs(session.addedIngredients))

  const ratedCount = feedbackItems.filter((f) => f.rating > 0).length

  function getRating(id1: string, id2: string): number {
    return feedbackItems.find((f) => f.ingredientId1 === id1 && f.ingredientId2 === id2)?.rating ?? 0
  }

  if (allPairs.length === 0) {
    return (
      <div data-testid="pairing-feedback-step">
        <h2 className="mb-1 text-xl font-semibold text-gray-900 dark:text-white">
          Pairing Feedback
        </h2>
        <p className="mb-6 text-sm text-gray-500 dark:text-gray-400">
          No ingredient pairs to rate in this session.
        </p>
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={onSkip}
            className="rounded-md border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
            aria-label="Skip pairing feedback"
            data-testid="feedback-skip-button"
          >
            Skip
          </button>
          <button
            type="button"
            onClick={onNext}
            className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
            aria-label="Continue to photo upload"
            data-testid="feedback-next-button"
          >
            Continue
          </button>
        </div>
      </div>
    )
  }

  return (
    <div data-testid="pairing-feedback-step">
      <h2 className="mb-1 text-xl font-semibold text-gray-900 dark:text-white">
        Pairing Feedback
      </h2>
      <p className="mb-2 text-sm text-gray-500 dark:text-gray-400">
        Rate how well these ingredient combinations worked together. Your feedback improves future
        suggestions.
      </p>
      {ratedCount > 0 && (
        <p className="mb-4 text-sm text-primary-600 dark:text-primary-400" data-testid="rated-count">
          {ratedCount} of {allPairs.length} pair{allPairs.length !== 1 ? 's' : ''} rated
        </p>
      )}

      <div className="mb-6 space-y-4">
        {allPairs.map(([a, b]) => {
          const rating = getRating(a.ingredientId, b.ingredientId)
          return (
            <div
              key={`${a.ingredientId}-${b.ingredientId}`}
              className={`rounded-lg border p-4 transition-colors ${
                rating > 0
                  ? 'border-primary-200 bg-primary-50 dark:border-primary-800 dark:bg-primary-950'
                  : 'border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800'
              }`}
              data-testid={`pair-${a.ingredientId}-${b.ingredientId}`}
            >
              <div className="mb-2 flex items-center justify-between">
                <span className="text-sm font-medium text-gray-900 dark:text-white">
                  {a.name} + {b.name}
                </span>
                {rating > 0 && (
                  <span className="text-xs text-primary-600 dark:text-primary-400" aria-live="polite">
                    ✓ Rated
                  </span>
                )}
              </div>
              <StarRating
                ingredientId1={a.ingredientId}
                ingredientId2={b.ingredientId}
                currentRating={rating}
                onRate={(r) => onRate(a.ingredientId, b.ingredientId, r)}
              />
            </div>
          )
        })}
      </div>

      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onSkip}
          className="rounded-md border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label="Skip pairing feedback"
          data-testid="feedback-skip-button"
        >
          Skip
        </button>
        <button
          type="button"
          onClick={onNext}
          className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Submit feedback and continue"
          data-testid="feedback-next-button"
        >
          {ratedCount > 0 ? 'Submit & Continue' : 'Continue'}
        </button>
      </div>
    </div>
  )
}
