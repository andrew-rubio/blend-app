'use client'

import { useRouter } from 'next/navigation'
import type { HomeRecentlyViewedRecipe } from '@/types'

export interface RecentlyViewedSectionProps {
  recipes: HomeRecentlyViewedRecipe[]
}

export function RecentlyViewedSection({ recipes }: RecentlyViewedSectionProps) {
  const router = useRouter()

  // Hidden when empty (HOME-24)
  if (recipes.length === 0) return null

  return (
    <section aria-labelledby="recently-viewed-heading">
      <h2 id="recently-viewed-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
        Recently Viewed
      </h2>
      <div
        className="flex gap-3 overflow-x-auto pb-2 scrollbar-hide"
        role="list"
        aria-label="Recently viewed recipes"
      >
        {recipes.map((recipe) => (
          <div key={recipe.recipeId} role="listitem" className="w-24 flex-shrink-0 sm:w-28">
            <RecentlyViewedCard
              recipe={recipe}
              onClick={() => router.push(`/recipes/${recipe.recipeId}`)}
            />
          </div>
        ))}
      </div>
    </section>
  )
}

interface RecentlyViewedCardProps {
  recipe: HomeRecentlyViewedRecipe
  onClick: () => void
}

function RecentlyViewedCard({ recipe, onClick }: RecentlyViewedCardProps) {
  return (
    <button
      onClick={onClick}
      aria-label={`View recipe ${recipe.recipeId}`}
      className="group w-full text-left focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 rounded-xl"
    >
      <div className="relative aspect-square w-full overflow-hidden rounded-xl bg-gray-100 dark:bg-gray-800">
        <div className="flex h-full w-full items-center justify-center text-gray-300 dark:text-gray-600" aria-hidden="true">
          <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
        {/* Hover overlay */}
        <div className="absolute inset-0 rounded-xl ring-2 ring-transparent group-hover:ring-orange-400 transition-all duration-200" aria-hidden="true" />
      </div>
      <p className="mt-1.5 line-clamp-2 text-center text-xs text-gray-700 dark:text-gray-300">
        {recipe.recipeId}
      </p>
    </button>
  )
}
