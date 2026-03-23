'use client'

import { useEffect } from 'react'
import { useIngredientDetail } from '@/hooks/useCookMode'

export interface IngredientDetailModalProps {
  sessionId: string
  ingredientId: string
  onClose: () => void
}

export function IngredientDetailModal({ sessionId, ingredientId, onClose }: IngredientDetailModalProps) {
  const { data, isLoading, error } = useIngredientDetail(sessionId, ingredientId)

  useEffect(() => {
    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleKey)
    return () => document.removeEventListener('keydown', handleKey)
  }, [onClose])

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      data-testid="ingredient-detail-modal"
    >
      <div
        className="absolute inset-0 bg-black/50"
        aria-hidden="true"
        onClick={onClose}
        data-testid="ingredient-detail-backdrop"
      />
      <div className="relative z-10 w-full max-w-md rounded-lg bg-white p-6 shadow-xl dark:bg-gray-900">
        <button
          type="button"
          onClick={onClose}
          aria-label="Close ingredient detail"
          className="absolute right-4 top-4 rounded p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800"
          data-testid="ingredient-detail-close"
        >
          ×
        </button>

        {isLoading && (
          <div className="animate-pulse" data-testid="ingredient-detail-loading">
            <div className="mb-4 h-6 w-1/2 rounded bg-gray-200 dark:bg-gray-700" />
            <div className="h-4 w-full rounded bg-gray-200 dark:bg-gray-700" />
          </div>
        )}

        {error && !isLoading && (
          <div data-testid="ingredient-detail-error">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              {(error as { message?: string })?.message ?? 'Ingredient'}
            </h2>
            <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">Knowledge Base unavailable</p>
          </div>
        )}

        {data && !isLoading && !error && (
          <div data-testid="ingredient-detail-content">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">{data.name}</h2>
            {data.category && (
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{data.category}</p>
            )}
            {data.flavourProfile && (
              <div className="mt-3">
                <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300">Flavour Profile</h3>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{data.flavourProfile}</p>
              </div>
            )}
            {data.whyItPairs && (
              <div className="mt-3">
                <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300">Why It Pairs</h3>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{data.whyItPairs}</p>
              </div>
            )}
            {data.substitutes.length > 0 && (
              <div className="mt-3">
                <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300">Substitutes</h3>
                <ul className="mt-1 list-disc pl-5 text-sm text-gray-600 dark:text-gray-400">
                  {data.substitutes.map((s) => (
                    <li key={s}>{s}</li>
                  ))}
                </ul>
              </div>
            )}
            {data.nutritionSummary && (
              <div className="mt-3">
                <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300">Nutrition</h3>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{data.nutritionSummary}</p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
