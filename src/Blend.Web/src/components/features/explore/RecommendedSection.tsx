'use client'

import { useRouter } from 'next/navigation'
import { RecipeCard } from './RecipeCard'
import { SkeletonGrid } from './SkeletonCard'
import { useRecommendedRecipes } from '@/hooks/useSearch'

/**
 * Grid of personalised recipe recommendations (EXPL-04).
 */
export function RecommendedSection() {
  const router = useRouter()
  const { data: recipes = [], isLoading, error } = useRecommendedRecipes()

  function handleCardClick(id: string) {
    router.push(`/recipes/${id}`)
  }

  return (
    <section aria-labelledby="recommended-heading">
      <h2
        id="recommended-heading"
        className="mb-3 text-lg font-semibold text-gray-900 dark:text-white"
      >
        Recommended for You
      </h2>

      {error && (
        <p className="text-sm text-red-600 dark:text-red-400" role="alert">
          Could not load recommendations.
        </p>
      )}

      {isLoading ? (
        <SkeletonGrid count={8} />
      ) : (
        <div
          className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4"
          role="list"
          aria-label="Recommended recipes"
        >
          {recipes.map((recipe) => (
            <div key={recipe.id} role="listitem">
              <RecipeCard recipe={recipe} onClick={handleCardClick} />
            </div>
          ))}
        </div>
      )}
    </section>
  )
}
