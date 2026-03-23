'use client'

import Image from 'next/image'
import { clsx } from 'clsx'
import type { RecipeSearchResult } from '@/types'

export interface RecipeCardProps {
  recipe: RecipeSearchResult
  onClick?: (id: string) => void
}

const sourceLabels = {
  Spoonacular: 'Spoonacular',
  Community: 'Community',
} as const

const sourceBadgeClasses = {
  Spoonacular:
    'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300',
  Community:
    'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
} as const

/**
 * A recipe result card showing image, title, cuisine tags, prep time, popularity,
 * and data-source badge (EXPL-13).
 */
export function RecipeCard({ recipe, onClick }: RecipeCardProps) {
  const {
    id,
    title,
    imageUrl,
    cuisines,
    dishTypes,
    readyInMinutes,
    popularity,
    dataSource,
  } = recipe

  return (
    <article
      role="article"
      aria-label={title}
      onClick={() => onClick?.(id)}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onClick?.(id)
        }
      }}
      tabIndex={0}
      className={clsx(
        'group flex cursor-pointer flex-col overflow-hidden rounded-xl border',
        'border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md',
        'dark:border-gray-700 dark:bg-gray-900',
        'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
      )}
    >
      {/* Thumbnail */}
      <div className="relative aspect-[4/3] w-full overflow-hidden bg-gray-100 dark:bg-gray-800">
        {imageUrl ? (
          <Image
            src={imageUrl}
            alt={title}
            fill
            sizes="(max-width: 768px) 50vw, (max-width: 1200px) 33vw, 25vw"
            className="object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-gray-300 dark:text-gray-600" aria-hidden="true">
            <svg className="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}

        {/* Data source badge */}
        <span
          aria-label={`Source: ${sourceLabels[dataSource]}`}
          className={clsx(
            'absolute right-2 top-2 rounded-full px-2 py-0.5 text-xs font-semibold',
            sourceBadgeClasses[dataSource]
          )}
        >
          {sourceLabels[dataSource]}
        </span>
      </div>

      {/* Content */}
      <div className="flex flex-1 flex-col gap-2 p-3">
        <h3 className="line-clamp-2 text-sm font-semibold text-gray-900 dark:text-gray-100 group-hover:text-primary-600">
          {title}
        </h3>

        {/* Cuisine tags */}
        {cuisines.length > 0 && (
          <div className="flex flex-wrap gap-1" aria-label="Cuisine tags">
            {cuisines.slice(0, 2).map((c) => (
              <span
                key={c}
                className="rounded-full bg-primary-50 px-2 py-0.5 text-xs text-primary-700 dark:bg-primary-900/30 dark:text-primary-300"
              >
                {c}
              </span>
            ))}
            {dishTypes.slice(0, 1).map((d) => (
              <span
                key={d}
                className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600 dark:bg-gray-800 dark:text-gray-400"
              >
                {d}
              </span>
            ))}
          </div>
        )}

        {/* Meta row */}
        <div className="mt-auto flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
          {readyInMinutes != null && (
            <span className="flex items-center gap-1" aria-label={`${readyInMinutes} minutes`}>
              <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {readyInMinutes} min
            </span>
          )}
          <span className="flex items-center gap-1" aria-label={`${popularity} likes`}>
            <svg className="h-3.5 w-3.5 text-pink-400" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
            </svg>
            {popularity.toLocaleString()}
          </span>
        </div>
      </div>
    </article>
  )
}
