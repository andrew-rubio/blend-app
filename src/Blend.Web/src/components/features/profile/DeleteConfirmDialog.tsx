'use client'

import { clsx } from 'clsx'

export interface DeleteConfirmDialogProps {
  title: string
  onConfirm: () => void
  onCancel: () => void
  isDeleting?: boolean
}

export function DeleteConfirmDialog({ title, onConfirm, onCancel, isDeleting }: DeleteConfirmDialogProps) {
  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Confirm recipe deletion"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl dark:bg-gray-900">
        <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Delete recipe?</h2>
        <p className="mb-1 text-sm text-gray-600 dark:text-gray-400">
          <span className="font-medium">&ldquo;{title}&rdquo;</span> will be removed.
        </p>
        <p className="text-sm text-gray-500 dark:text-gray-500">
          You have 30 days to contact support for recovery.
        </p>

        <div className="mt-6 flex justify-end gap-3">
          <button
            onClick={onCancel}
            disabled={isDeleting}
            aria-label="Cancel deletion"
            className={clsx(
              'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
              'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
              'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
              'disabled:opacity-50'
            )}
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isDeleting}
            aria-label="Confirm delete recipe"
            className={clsx(
              'rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white',
              'hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2',
              'disabled:opacity-50'
            )}
          >
            {isDeleting ? 'Deleting…' : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  )
}
