import { IngredientTypeahead } from '../IngredientTypeahead'

export interface IngredientsStepProps {
  addedIds: string[]
  onAdd: (id: string) => void
  onRemove: (id: string) => void
}

/** Step 5 — Disliked ingredients typeahead. */
export function IngredientsStep({ addedIds, onAdd, onRemove }: IngredientsStepProps) {
  return (
    <div>
      <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
        Add any specific ingredients you dislike or want to avoid. These will be excluded when
        generating cook-mode suggestions.
      </p>
      <IngredientTypeahead addedIds={addedIds} onAdd={onAdd} onRemove={onRemove} />
    </div>
  )
}
