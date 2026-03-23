'use client'

import type { Recipe } from '@/types'

interface OverviewTabProps {
  recipe: Recipe
}

export function OverviewTab({ recipe }: OverviewTabProps) {
  const totalTime =
    recipe.readyInMinutes ??
    ((recipe.prepTimeMinutes ?? 0) + (recipe.cookTimeMinutes ?? 0) || undefined)

  return (
    <div className="flex flex-col gap-6">
      {/* Description */}
      {recipe.description && (
        <p className="text-base leading-relaxed text-gray-700 dark:text-gray-300">
          {recipe.description}
        </p>
      )}

      {/* Key stats */}
      <div
        className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5"
        aria-label="Recipe stats"
      >
        {recipe.prepTimeMinutes != null && (
          <StatCard label="Prep" value={`${recipe.prepTimeMinutes} min`} />
        )}
        {recipe.cookTimeMinutes != null && (
          <StatCard label="Cook" value={`${recipe.cookTimeMinutes} min`} />
        )}
        {totalTime != null && <StatCard label="Total" value={`${totalTime} min`} />}
        <StatCard label="Servings" value={String(recipe.servings)} />
        {recipe.difficulty && <StatCard label="Difficulty" value={recipe.difficulty} />}
      </div>

      {/* Diet badges */}
      {recipe.diets.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
            Diets
          </h3>
          <div className="flex flex-wrap gap-2">
            {recipe.diets.map((diet) => (
              <Badge key={diet} label={diet} color="green" />
            ))}
          </div>
        </div>
      )}

      {/* Intolerance badges */}
      {recipe.intolerances.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
            Free From
          </h3>
          <div className="flex flex-wrap gap-2">
            {recipe.intolerances.map((item) => (
              <Badge key={item} label={item} color="blue" />
            ))}
          </div>
        </div>
      )}

      {/* Photo gallery */}
      {recipe.photos && recipe.photos.length > 1 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
            Photos
          </h3>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {recipe.photos.slice(1).map((url, i) => (
              <div key={i} className="aspect-video overflow-hidden rounded-lg bg-gray-100 dark:bg-gray-800">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={url}
                  alt={`${recipe.title} photo ${i + 2}`}
                  className="h-full w-full object-cover"
                />
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col items-center rounded-xl border border-gray-200 p-3 text-center dark:border-gray-700">
      <span className="text-sm text-gray-500 dark:text-gray-400">{label}</span>
      <span className="mt-1 text-base font-semibold text-gray-900 dark:text-white">{value}</span>
    </div>
  )
}

function Badge({ label, color }: { label: string; color: 'green' | 'blue' }) {
  const colorClass =
    color === 'green'
      ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300'
      : 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300'
  return (
    <span
      className={`rounded-full px-3 py-1 text-xs font-medium capitalize ${colorClass}`}
    >
      {label}
    </span>
  )
}
