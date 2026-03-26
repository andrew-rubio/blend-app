'use client'

import { clsx } from 'clsx'
import { useMyIngredientSubmissions } from '@/hooks/useIngredientSubmissions'
import type { IngredientSubmissionStatus } from '@/types'

const STATUS_LABELS: Record<IngredientSubmissionStatus, string> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
}

const STATUS_CLASSES: Record<IngredientSubmissionStatus, string> = {
  Pending: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-300',
  Approved: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300',
  Rejected: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300',
}

/**
 * Displays the current user's ingredient submissions with their status (SETT-11, SETT-12).
 */
export function MySubmissions() {
  const { data, isLoading, error } = useMyIngredientSubmissions()

  if (isLoading) {
    return (
      <ul aria-busy="true" aria-label="Loading submissions" className="space-y-2">
        {[1, 2].map((i) => (
          <li key={i} className="animate-pulse rounded-lg bg-gray-100 py-6 dark:bg-gray-800" />
        ))}
      </ul>
    )
  }

  if (error) {
    const message =
      error && typeof error === 'object' && 'message' in error
        ? (error as { message: string }).message
        : 'Could not load submissions.'
    return (
      <div role="alert" className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300">
        {message}
      </div>
    )
  }

  const submissions = data?.submissions ?? []

  if (submissions.length === 0) {
    return (
      <p className="text-sm text-gray-500 dark:text-gray-400">
        You haven&apos;t submitted any ingredients yet.
      </p>
    )
  }

  return (
    <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800" aria-label="My ingredient submissions">
      {submissions.map((submission) => (
        <li key={submission.id} className="px-4 py-3">
          <div className="flex items-center justify-between gap-2">
            <div className="min-w-0">
              <p className="truncate font-medium text-gray-900 dark:text-white">{submission.name}</p>
              <p className="text-xs text-gray-500 dark:text-gray-400">{submission.category}</p>
            </div>
            <span
              className={clsx(
                'flex-shrink-0 rounded-full px-2 py-0.5 text-xs font-medium',
                STATUS_CLASSES[submission.status]
              )}
            >
              {STATUS_LABELS[submission.status]}
            </span>
          </div>
          {submission.reviewNotes && (
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Note: {submission.reviewNotes}
            </p>
          )}
        </li>
      ))}
    </ul>
  )
}
