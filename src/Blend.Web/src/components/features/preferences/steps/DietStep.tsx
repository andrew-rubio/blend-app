import { SelectionChip } from '../SelectionChip'

const DIET_DESCRIPTIONS: Record<string, string> = {
  'gluten free': 'Excludes any food containing gluten.',
  ketogenic: 'Very low-carb, high-fat diet.',
  vegetarian: 'No meat; dairy and eggs allowed.',
  'lacto-vegetarian': 'Vegetarian + dairy; no eggs.',
  'ovo-vegetarian': 'Vegetarian + eggs; no dairy.',
  vegan: 'No animal products whatsoever.',
  pescetarian: 'Vegetarian + fish and seafood.',
  paleo: 'Whole foods; no grains, dairy or processed food.',
  primal: 'Like paleo but allows dairy.',
  'low FODMAP': 'Limits fermentable carbohydrates for IBS relief.',
  whole30: '30-day elimination diet of sugar, grains, dairy and legumes.',
}

export interface DietStepProps {
  diets: string[]
  selected: string[]
  onToggle: (diet: string) => void
  isLoading?: boolean
}

/** Step 3 — Dietary preferences list. */
export function DietStep({ diets, selected, onToggle, isLoading = false }: DietStepProps) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" aria-live="polite" aria-busy="true">
        <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-600 border-t-transparent" />
        <span className="sr-only">Loading diets…</span>
      </div>
    )
  }

  return (
    <div>
      <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
        Select any dietary plans you follow. These will be used to filter recipe suggestions.
      </p>
      <ul className="flex flex-col gap-2" role="group" aria-label="Dietary preference selection">
        {diets.map((diet) => (
          <li key={diet} className="flex items-start gap-3">
            <div className="pt-0.5">
              <SelectionChip label={diet} selected={selected.includes(diet)} onClick={() => onToggle(diet)} />
            </div>
            {DIET_DESCRIPTIONS[diet] && (
              <p className="pt-1.5 text-sm text-gray-500 dark:text-gray-400">
                {DIET_DESCRIPTIONS[diet]}
              </p>
            )}
          </li>
        ))}
      </ul>
      {selected.length > 0 && (
        <p className="mt-3 text-sm text-primary-600 dark:text-primary-400" aria-live="polite">
          {selected.length} selected
        </p>
      )}
    </div>
  )
}
