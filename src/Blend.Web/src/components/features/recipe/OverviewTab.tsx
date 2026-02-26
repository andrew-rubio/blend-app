import type { Recipe } from '@/types/recipe'
import Image from 'next/image'

interface Props { recipe: Recipe }

function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="flex flex-col items-center rounded-lg bg-gray-50 p-3">
      <span className="text-lg font-semibold text-gray-900">{value}</span>
      <span className="text-xs text-gray-500">{label}</span>
    </div>
  )
}

export function OverviewTab({ recipe }: Props) {
  return (
    <div className="space-y-6">
      {recipe.description && <p className="text-gray-700">{recipe.description}</p>}
      <div className="grid grid-cols-3 gap-3 sm:grid-cols-5">
        <Stat label="Prep" value={`${recipe.prepTimeMinutes}m`} />
        <Stat label="Cook" value={`${recipe.cookTimeMinutes}m`} />
        <Stat label="Total" value={`${recipe.totalTimeMinutes}m`} />
        <Stat label="Servings" value={recipe.servings} />
        <Stat label="Difficulty" value={recipe.difficulty} />
      </div>
      {recipe.diets.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500">Diets</h3>
          <div className="flex flex-wrap gap-2">
            {recipe.diets.map((d) => (
              <span key={d} className="rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-700">
                {d}
              </span>
            ))}
          </div>
        </div>
      )}
      {recipe.intolerances.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500">Free from</h3>
          <div className="flex flex-wrap gap-2">
            {recipe.intolerances.map((i) => (
              <span key={i} className="rounded-full bg-blue-100 px-3 py-1 text-xs font-medium text-blue-700">
                {i}-free
              </span>
            ))}
          </div>
        </div>
      )}
      {recipe.images && recipe.images.length > 1 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500">Gallery</h3>
          <div className="grid grid-cols-3 gap-2">
            {recipe.images.map((src, i) => (
              <div key={i} className="relative aspect-square overflow-hidden rounded-lg">
                <Image src={src} alt={`Photo ${i + 1}`} fill className="object-cover" />
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
