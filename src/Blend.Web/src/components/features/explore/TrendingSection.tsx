'use client'

import { useRouter } from 'next/navigation'
import { RecipeCard } from './RecipeCard'
import { SkeletonCard } from './SkeletonCard'
import { useTrendingRecipes } from '@/hooks/useSearch'

/**
 * Horizontally scrollable trending recipes section (EXPL-02).
 */
export function TrendingSection() {
  const router = useRouter()
  const { data: recipes = [], isLoading, error } = useTrendingRecipes()

  function handleCardClick(id: string) {
    router.push(`/recipes/${id}`)
  }

  return (
    <section aria-labelledby="trending-heading">
      <h2
        id="trending-heading"
        className="mb-3 text-lg font-semibold text-gray-900 dark:text-white"
      >
        Trending Recipes
      </h2>

      {error && (
        <p className="text-sm text-red-600 dark:text-red-400" role="alert">
          Could not load trending recipes.
        </p>
      )}

      <div
        className="flex gap-4 overflow-x-auto pb-2 scrollbar-hide"
        role="list"
        aria-label="Trending recipes"
      >
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="w-48 flex-shrink-0 sm:w-56">
                <SkeletonCard />
              </div>
            ))
          : recipes.map((recipe) => (
              <div key={recipe.id} role="listitem" className="w-48 flex-shrink-0 sm:w-56">
                <RecipeCard recipe={recipe} onClick={handleCardClick} />
              </div>
            ))}
      </div>
    </section>
  )
}
