'use client'

import Link from 'next/link'

interface CompletionStepProps {
  publishedRecipeId: string | null
  onReturnHome: () => void
}

/**
 * Step 5: Completion (COOK-45).
 * Success message with optional link to the published recipe and a return-to-home action.
 */
export function CompletionStep({ publishedRecipeId, onReturnHome }: CompletionStepProps) {
  return (
    <div className="flex flex-col items-center py-8 text-center" data-testid="completion-step">
      <span className="mb-4 text-5xl" aria-hidden="true">
        🎉
      </span>

      <h2 className="mb-2 text-2xl font-bold text-gray-900 dark:text-white">
        {publishedRecipeId ? 'Recipe Published!' : 'Session Complete!'}
      </h2>

      <p className="mb-8 max-w-sm text-sm text-gray-500 dark:text-gray-400">
        {publishedRecipeId
          ? 'Your recipe has been shared with the Blend community. Thank you for contributing!'
          : 'Great job! Your cooking session is complete. Keep experimenting!'}
      </p>

      {publishedRecipeId && (
        <Link
          href={`/recipes/${publishedRecipeId}`}
          className="mb-4 rounded-md bg-primary-600 px-6 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="View published recipe"
          data-testid="view-recipe-link"
        >
          View Your Recipe
        </Link>
      )}

      <button
        type="button"
        onClick={onReturnHome}
        className="rounded-md border border-gray-300 px-6 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
        aria-label="Return to home"
        data-testid="return-home-button"
      >
        Return to Home
      </button>
    </div>
  )
}
