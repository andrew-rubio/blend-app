import { SelectionChip } from '../SelectionChip'

export interface IntoleranceStepProps {
  intolerances: string[]
  selected: string[]
  onToggle: (intolerance: string) => void
  isLoading?: boolean
}

/** Step 4 — Intolerance selection. Selected items are strictly excluded from results (PREF-07). */
export function IntoleranceStep({
  intolerances,
  selected,
  onToggle,
  isLoading = false,
}: IntoleranceStepProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" aria-live="polite" aria-busy="true">
        <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-600 border-t-transparent" />
        <span className="sr-only">Loading intolerances…</span>
      </div>
    )
  }

  return (
    <div>
      <div
        role="alert"
        className="mb-4 flex items-start gap-2 rounded-md border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-800 dark:border-amber-700 dark:bg-amber-900/30 dark:text-amber-200"
      >
        <svg
          className="mt-0.5 h-4 w-4 flex-shrink-0"
          fill="currentColor"
          viewBox="0 0 20 20"
          aria-hidden="true"
        >
          <path
            fillRule="evenodd"
            d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
            clipRule="evenodd"
          />
        </svg>
        <span>
          <strong>Strict exclusion:</strong> Selected intolerances will completely exclude any
          recipe containing these allergens from your results.
        </span>
      </div>
      <div role="group" aria-label="Intolerance selection" className="flex flex-wrap gap-2">
        {intolerances.map((intolerance) => (
          <SelectionChip
            key={intolerance}
            label={intolerance}
            selected={selected.includes(intolerance)}
            onClick={() => onToggle(intolerance)}
          />
        ))}
      </div>
      {selected.length > 0 && (
        <p className="mt-3 text-sm text-primary-600 dark:text-primary-400" aria-live="polite">
          {selected.length} intolerance{selected.length !== 1 ? 's' : ''} selected — these
          ingredients will be strictly excluded.
        </p>
      )}
    </div>
  )
}
