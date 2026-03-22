import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export interface SelectionChipProps {
  label: string
  selected: boolean
  onClick: () => void
  disabled?: boolean
}

/**
 * A toggleable chip used in the preference selection grids.
 * Shows a clear selected / unselected visual state (PREF-04).
 */
export function SelectionChip({ label, selected, onClick, disabled = false }: SelectionChipProps) {
  return (
    <button
      type="button"
      role="checkbox"
      aria-checked={selected}
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className={twMerge(
        clsx(
          'inline-flex items-center justify-center rounded-full border px-4 py-2 text-sm font-medium transition-colors',
          'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
          'disabled:cursor-not-allowed disabled:opacity-50',
          selected
            ? 'border-primary-600 bg-primary-600 text-white hover:bg-primary-700'
            : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-300 dark:hover:bg-gray-800'
        )
      )}
    >
      {selected && (
        <svg
          className="mr-1.5 h-3.5 w-3.5"
          fill="currentColor"
          viewBox="0 0 20 20"
          aria-hidden="true"
        >
          <path
            fillRule="evenodd"
            d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
            clipRule="evenodd"
          />
        </svg>
      )}
      {label}
    </button>
  )
}
