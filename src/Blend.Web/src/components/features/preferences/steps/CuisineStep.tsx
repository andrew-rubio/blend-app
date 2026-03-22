import { SelectionChip } from '../SelectionChip'

export interface CuisineStepProps {
  cuisines: string[]
  selected: string[]
  onToggle: (cuisine: string) => void
  isLoading?: boolean
}

/** Step 1 — Cuisine selection grid. */
export function CuisineStep({ cuisines, selected, onToggle, isLoading = false }: CuisineStepProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" aria-live="polite" aria-busy="true">
        <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-600 border-t-transparent" />
        <span className="sr-only">Loading cuisines…</span>
      </div>
    )
  }

  return (
    <div>
      <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
        Select the cuisines you enjoy most. You can change these at any time.
      </p>
      <div
        role="group"
        aria-label="Cuisine selection"
        className="flex flex-wrap gap-2"
      >
        {cuisines.map((cuisine) => (
          <SelectionChip
            key={cuisine}
            label={cuisine}
            selected={selected.includes(cuisine)}
            onClick={() => onToggle(cuisine)}
          />
        ))}
      </div>
      {selected.length > 0 && (
        <p className="mt-3 text-sm text-primary-600 dark:text-primary-400" aria-live="polite">
          {selected.length} selected
        </p>
      )}
    </div>
  )
}
