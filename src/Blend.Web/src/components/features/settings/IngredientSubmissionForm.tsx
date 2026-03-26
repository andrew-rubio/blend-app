'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import { useCreateIngredientSubmission } from '@/hooks/useIngredientSubmissions'
import type { IngredientCategory } from '@/types'

const CATEGORIES: IngredientCategory[] = [
  'Produce',
  'Meat',
  'Seafood',
  'Dairy',
  'Grains',
  'Spices',
  'Condiments',
  'Beverages',
  'Other',
]

const NAME_MIN = 2
const NAME_MAX = 100
const DESCRIPTION_MAX = 500

export interface IngredientSubmissionFormProps {
  onClose: () => void
}

/**
 * Form for submitting a new ingredient to the knowledge base (SETT-08 through SETT-12).
 */
export function IngredientSubmissionForm({ onClose }: IngredientSubmissionFormProps) {
  const [name, setName] = useState('')
  const [category, setCategory] = useState<IngredientCategory>('Produce')
  const [description, setDescription] = useState('')
  const [fieldErrors, setFieldErrors] = useState<{ name?: string; description?: string }>({})
  const [submitted, setSubmitted] = useState(false)

  const { mutate: submit, isPending, error } = useCreateIngredientSubmission()

  function validate(): boolean {
    const errors: { name?: string; description?: string } = {}
    if (name.trim().length < NAME_MIN || name.trim().length > NAME_MAX) {
      errors.name = `Ingredient name must be between ${NAME_MIN} and ${NAME_MAX} characters.`
    }
    if (description.length > DESCRIPTION_MAX) {
      errors.description = `Description must be at most ${DESCRIPTION_MAX} characters.`
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  function handleSubmit() {
    if (!validate()) return
    submit(
      { name: name.trim(), category, description: description.trim() || undefined },
      {
        onSuccess: () => setSubmitted(true),
      }
    )
  }

  const errorMessage =
    error && typeof error === 'object' && 'message' in error
      ? (error as { message: string }).message
      : null

  if (submitted) {
    return (
      <div className="rounded-xl border border-green-200 bg-green-50 p-6 dark:border-green-800 dark:bg-green-900/30">
        <div className="mb-2 flex items-center gap-2">
          <svg className="h-5 w-5 text-green-600 dark:text-green-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
          </svg>
          <h3 className="font-semibold text-green-800 dark:text-green-300">Submission received!</h3>
        </div>
        <p className="mb-4 text-sm text-green-700 dark:text-green-400">
          Your ingredient has been submitted for review. You can track its status under &quot;My Submissions&quot;.
        </p>
        <button
          type="button"
          onClick={onClose}
          className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
        >
          Done
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Ingredient name */}
      <div>
        <label htmlFor="submission-name" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Ingredient name <span aria-hidden="true">*</span>
        </label>
        <input
          id="submission-name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          disabled={isPending}
          aria-describedby={fieldErrors.name ? 'submission-name-error' : undefined}
          placeholder="e.g. Dragonfruit"
          className={clsx(
            'mt-1 block w-full rounded-lg border px-3 py-2 text-sm',
            'focus:outline-none focus:ring-2 focus:ring-primary-500',
            'dark:bg-gray-800 dark:text-white',
            fieldErrors.name
              ? 'border-red-400 dark:border-red-500'
              : 'border-gray-300 dark:border-gray-600',
            'disabled:opacity-50'
          )}
        />
        {fieldErrors.name && (
          <p id="submission-name-error" role="alert" className="mt-1 text-xs text-red-500">
            {fieldErrors.name}
          </p>
        )}
      </div>

      {/* Category */}
      <div>
        <label htmlFor="submission-category" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Category <span aria-hidden="true">*</span>
        </label>
        <select
          id="submission-category"
          value={category}
          onChange={(e) => setCategory(e.target.value as IngredientCategory)}
          disabled={isPending}
          className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white disabled:opacity-50"
        >
          {CATEGORIES.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
      </div>

      {/* Description */}
      <div>
        <label htmlFor="submission-description" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Description
        </label>
        <textarea
          id="submission-description"
          rows={3}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          disabled={isPending}
          placeholder="Describe the ingredient, its taste, or culinary uses…"
          aria-describedby={fieldErrors.description ? 'submission-description-error' : 'submission-description-count'}
          className={clsx(
            'mt-1 block w-full resize-none rounded-lg border px-3 py-2 text-sm',
            'focus:outline-none focus:ring-2 focus:ring-primary-500',
            'dark:bg-gray-800 dark:text-white',
            fieldErrors.description
              ? 'border-red-400 dark:border-red-500'
              : 'border-gray-300 dark:border-gray-600',
            'disabled:opacity-50'
          )}
        />
        <p id="submission-description-count" className="mt-1 text-xs text-gray-400 dark:text-gray-500">
          {description.length} / {DESCRIPTION_MAX}
        </p>
        {fieldErrors.description && (
          <p id="submission-description-error" role="alert" className="mt-1 text-xs text-red-500">
            {fieldErrors.description}
          </p>
        )}
      </div>

      {errorMessage && (
        <div role="alert" className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300">
          {errorMessage}
        </div>
      )}

      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onClose}
          disabled={isPending}
          className={clsx(
            'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
            'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50'
          )}
        >
          Cancel
        </button>
        <button
          type="button"
          onClick={handleSubmit}
          disabled={isPending}
          className={clsx(
            'rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white',
            'hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50',
            isPending && 'cursor-not-allowed'
          )}
        >
          {isPending ? 'Submitting…' : 'Submit'}
        </button>
      </div>
    </div>
  )
}
