import { SelectionChip } from '../SelectionChip'

export interface DishTypeStepProps {
  dishTypes: string[]
  selected: string[]
  onToggle: (dishType: string) => void
  isLoading?: boolean
}

/** Step 2 — Dish type selection grid. */
export function DishTypeStep({
  dishTypes,
  selected,
  onToggle,
  isLoading = false,
}: DishTypeStepProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" aria-live="polite" aria-busy="true">
        <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-600 border-t-transparent" />
        <span className="sr-only">Loading dish types…</span>
      </div>
    )
  }

  return (
    <div>
      <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
        Select the types of dishes you like to cook or eat.
      </p>
      <div role="group" aria-label="Dish type selection" className="flex flex-wrap gap-2">
        {dishTypes.map((dishType) => (
          <SelectionChip
            key={dishType}
            label={dishType}
            selected={selected.includes(dishType)}
            onClick={() => onToggle(dishType)}
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
