'use client'

import Image from 'next/image'
import { clsx } from 'clsx'
import type { ProfileRecipe } from '@/types'
import { RecipeContextMenu } from './RecipeContextMenu'

export interface ProfileRecipeCardProps {
  recipe: ProfileRecipe
  showContextMenu?: boolean
  onCardClick?: (id: string) => void
  onEdit?: (id: string) => void
  onToggleVisibility?: (id: string, currentIsPublic: boolean) => void
  onDelete?: (id: string) => void
}

export function ProfileRecipeCard({
  recipe,
  showContextMenu = false,
  onCardClick,
  onEdit,
  onToggleVisibility,
  onDelete,
}: ProfileRecipeCardProps) {
  const { id, title, imageUrl, cuisines, likeCount, isPublic } = recipe

  return (
    <article
      aria-label={title}
      className={clsx(
        'group flex flex-col overflow-hidden rounded-xl border',
        'border-gray-200 bg-white shadow-sm',
        'dark:border-gray-700 dark:bg-gray-900'
      )}
    >
      {/* Thumbnail */}
      <div
        className="relative aspect-[4/3] w-full cursor-pointer overflow-hidden bg-gray-100 dark:bg-gray-800"
        onClick={() => onCardClick?.(id)}
        role="button"
        tabIndex={0}
        aria-label={`Open ${title}`}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault()
            onCardClick?.(id)
          }
        }}
      >
        {imageUrl ? (
          <Image
            src={imageUrl}
            alt={title}
            fill
            sizes="(max-width: 768px) 50vw, 33vw"
            className="object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-gray-300 dark:text-gray-600" aria-hidden="true">
            <svg className="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}

        {/* Visibility badge */}
        <span
          aria-label={isPublic ? 'Public recipe' : 'Private recipe'}
          className={clsx(
            'absolute left-2 top-2 flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold',
            isPublic
              ? 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300'
              : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400'
          )}
        >
          {isPublic ? (
            <>
              <svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
              Public
            </>
          ) : (
            <>
              <svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              Private
            </>
          )}
        </span>
      </div>

      {/* Content */}
      <div className="flex flex-1 flex-col gap-1 p-3">
        <div className="flex items-start justify-between gap-2">
          <h3 className="line-clamp-2 flex-1 text-sm font-semibold text-gray-900 dark:text-gray-100">
            {title}
          </h3>
          {showContextMenu && onEdit && onToggleVisibility && onDelete && (
            <RecipeContextMenu
              recipe={recipe}
              onEdit={onEdit}
              onToggleVisibility={onToggleVisibility}
              onDelete={onDelete}
            />
          )}
        </div>

        {cuisines.length > 0 && (
          <div className="flex flex-wrap gap-1" aria-label="Cuisine tags">
            {cuisines.slice(0, 2).map((c) => (
              <span key={c} className="rounded-full bg-primary-50 px-2 py-0.5 text-xs text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                {c}
              </span>
            ))}
          </div>
        )}

        <div className="mt-auto flex items-center text-xs text-gray-500 dark:text-gray-400">
          <span className="flex items-center gap-1" aria-label={`${likeCount} likes`}>
            <svg className="h-3.5 w-3.5 text-pink-400" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
            </svg>
            {likeCount.toLocaleString()}
          </span>
        </div>
      </div>
    </article>
  )
}
