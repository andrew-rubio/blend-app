import Image from 'next/image'
import { Badge } from '@/components/ui/Badge'
import type { Recipe } from '@/types/recipe'

interface OverviewTabProps {
  recipe: Recipe
}

const difficultyLabel: Record<Recipe['difficulty'], string> = {
  easy: '🟢 Easy',
  medium: '🟡 Medium',
  hard: '🔴 Hard',
}

export function OverviewTab({ recipe }: OverviewTabProps) {
  return (
    <div className="space-y-6">
      <p className="text-gray-700 leading-relaxed">{recipe.description}</p>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <Stat label="Prep Time" value={`${recipe.prepTimeMinutes} min`} />
        <Stat label="Cook Time" value={`${recipe.cookTimeMinutes} min`} />
        <Stat label="Total Time" value={`${recipe.totalTimeMinutes} min`} />
        <Stat label="Servings" value={String(recipe.servings)} />
      </div>

      <div className="flex items-center gap-2">
        <span className="text-sm font-medium text-gray-700">Difficulty:</span>
        <span className="text-sm">{difficultyLabel[recipe.difficulty]}</span>
      </div>

      {recipe.diets.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-medium text-gray-700">Diets</h3>
          <div className="flex flex-wrap gap-2">
            {recipe.diets.map((diet) => (
              <Badge key={diet} variant="diet">
                {diet}
              </Badge>
            ))}
          </div>
        </div>
      )}

      {recipe.intolerances.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-medium text-gray-700">Free from</h3>
          <div className="flex flex-wrap gap-2">
            {recipe.intolerances.map((intolerance) => (
              <Badge key={intolerance} variant="intolerance">
                {intolerance}
              </Badge>
            ))}
          </div>
        </div>
      )}

      {recipe.images && recipe.images.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-medium text-gray-700">Photos</h3>
          <div className="flex gap-3 overflow-x-auto pb-2">
            {recipe.images.map((img, i) => (
              <div key={i} className="relative h-32 w-48 flex-shrink-0 overflow-hidden rounded-lg">
                <Image src={img} alt={`Recipe photo ${i + 1}`} fill className="object-cover" />
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-gray-50 p-3 text-center">
      <div className="text-lg font-semibold text-gray-900">{value}</div>
      <div className="text-xs text-gray-500">{label}</div>
    </div>
  )
}
