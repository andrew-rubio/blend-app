'use client'

import { useCancelAccountDeletion } from '@/hooks/useSettings'
import { useSettingsStore } from '@/stores/settingsStore'

/**
 * Banner shown when a deletion request is pending (SETT-24).
 * Allows the user to cancel within the 30-day grace period.
 */
export function DeletionCancellationBanner() {
  const pendingDeletionDate = useSettingsStore((s) => s.pendingDeletionDate)
  const { mutate: cancelDeletion, isPending, error } = useCancelAccountDeletion()

  if (!pendingDeletionDate) return null

  const formattedDate = new Intl.DateTimeFormat(undefined, {
    dateStyle: 'long',
  }).format(new Date(pendingDeletionDate))

  const errorMessage =
    error && typeof error === 'object' && 'message' in error
      ? (error as { message: string }).message
      : null

  return (
    <div
      role="alert"
      className="rounded-lg border border-red-300 bg-red-50 px-4 py-3 dark:border-red-700 dark:bg-red-900/30"
    >
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="font-medium text-red-800 dark:text-red-300">Account deletion scheduled</p>
          <p className="text-sm text-red-700 dark:text-red-400">
            Your account is scheduled for permanent deletion on <strong>{formattedDate}</strong>.
          </p>
        </div>
        <button
          type="button"
          onClick={() => cancelDeletion()}
          disabled={isPending}
          className="flex-shrink-0 rounded-lg border border-red-600 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:opacity-50 dark:border-red-500 dark:text-red-300 dark:hover:bg-red-900/50"
        >
          {isPending ? 'Cancelling…' : 'Cancel deletion'}
        </button>
      </div>
      {errorMessage && (
        <p className="mt-2 text-sm text-red-700 dark:text-red-400">{errorMessage}</p>
      )}
    </div>
  )
}
