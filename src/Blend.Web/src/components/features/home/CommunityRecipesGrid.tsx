'use client'

import Image from 'next/image'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { clsx } from 'clsx'
import type { HomeCommunityRecipe } from '@/types'

export interface CommunityRecipesGridProps {
  recipes: HomeCommunityRecipe[]
}

export function CommunityRecipesGrid({ recipes }: CommunityRecipesGridProps) {
  const router = useRouter()

  if (recipes.length === 0) return null

  return (
    <section aria-labelledby="community-heading">
      <div className="mb-3 flex items-center justify-between">
        <h2 id="community-heading" className="text-lg font-semibold text-gray-900 dark:text-white">
          Community Recipes
        </h2>
        <Link
          href="/explore?source=community"
          className="text-sm font-medium text-orange-600 hover:text-orange-700 dark:text-orange-400 dark:hover:text-orange-300 transition-colors"
          aria-label="See all community recipes"
        >
          See all
        </Link>
      </div>
      <div
        className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:gap-4"
        role="list"
        aria-label="Community recipes"
      >
        {recipes.map((recipe) => (
          <div key={recipe.id} role="listitem">
            <CommunityRecipeCard
              recipe={recipe}
              onClick={() => router.push(`/recipes/${recipe.id}`)}
            />
          </div>
        ))}
      </div>
    </section>
  )
}

interface CommunityRecipeCardProps {
  recipe: HomeCommunityRecipe
  onClick: () => void
}

function CommunityRecipeCard({ recipe, onClick }: CommunityRecipeCardProps) {
  return (
    <article
      role="article"
      aria-label={recipe.title}
      onClick={onClick}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onClick()
        }
      }}
      tabIndex={0}
      className={clsx(
        'group cursor-pointer overflow-hidden rounded-xl border',
        'border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md',
        'dark:border-gray-700 dark:bg-gray-900',
        'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
      )}
    >
      <div className="relative aspect-square w-full overflow-hidden bg-gray-100 dark:bg-gray-800">
        {recipe.imageUrl ? (
          <Image
            src={recipe.imageUrl}
            alt={recipe.title}
            fill
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
            className="object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-gray-300 dark:text-gray-600" aria-hidden="true">
            <svg className="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}
      </div>
      <div className="p-2.5">
        <h3 className="line-clamp-2 text-xs font-semibold text-gray-900 dark:text-white sm:text-sm">
          {recipe.title}
        </h3>
        <div className="mt-1.5 flex items-center justify-between">
          {recipe.cuisineType && (
            <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600 dark:bg-gray-800 dark:text-gray-400">
              {recipe.cuisineType}
            </span>
          )}
          <div className="flex items-center gap-1 text-xs text-gray-500 dark:text-gray-400 ml-auto">
            <svg className="h-3.5 w-3.5 text-red-400" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
            </svg>
            <span aria-label={`${recipe.likeCount} likes`}>{recipe.likeCount}</span>
          </div>
        </div>
      </div>
    </article>
  )
}

export function CommunityRecipesGridSkeleton() {
  return (
    <section aria-hidden="true">
      <div className="mb-3 flex items-center justify-between">
        <div className="h-6 w-40 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        <div className="h-4 w-12 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
      </div>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <div key={i} className="overflow-hidden rounded-xl border border-gray-200 dark:border-gray-700">
            <div className="aspect-square w-full animate-pulse bg-gray-200 dark:bg-gray-800" />
            <div className="p-2.5 flex flex-col gap-1.5">
              <div className="h-4 w-3/4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
              <div className="h-3 w-1/2 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
