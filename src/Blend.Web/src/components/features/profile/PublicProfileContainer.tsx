'use client'

import Image from 'next/image'
import { clsx } from 'clsx'
import { usePublicProfile, usePublicUserRecipes } from '@/hooks/useProfile'
import { ProfileRecipeCard } from './ProfileRecipeCard'

export interface PublicProfileContainerProps {
  userId: string
}

export function PublicProfileContainer({ userId }: PublicProfileContainerProps) {
  const { data: profile, isLoading: profileLoading, error: profileError } = usePublicProfile(userId)
  const {
    data: recipesData,
    isLoading: recipesLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = usePublicUserRecipes(userId)

  const recipes = recipesData?.pages.flatMap((p) => p.recipes) ?? []

  if (profileLoading) {
    return (
      <div aria-label="Loading profile" className="mx-auto max-w-4xl px-4 py-8">
        <div className="animate-pulse space-y-4">
          <div className="h-40 rounded-xl bg-gray-100 dark:bg-gray-800" />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/3] rounded-xl bg-gray-100 dark:bg-gray-800" />
            ))}
          </div>
        </div>
      </div>
    )
  }

  if (profileError || !profile) {
    const errStatus = profileError && typeof profileError === 'object' && 'status' in profileError
      ? (profileError as { status: number }).status
      : 0
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 text-center">
        <p className="text-gray-500 dark:text-gray-400">
          {errStatus === 404 ? 'User not found.' : 'Failed to load profile.'}
        </p>
      </div>
    )
  }

  const joinYear = new Date(profile.joinDate).getFullYear()

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Public profile header */}
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-700 dark:bg-gray-900">
        <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-start">
          <div className="relative h-20 w-20 flex-shrink-0 overflow-hidden rounded-full bg-gray-100 dark:bg-gray-800">
            {profile.avatarUrl ? (
              <Image
                src={profile.avatarUrl}
                alt={`${profile.displayName} avatar`}
                fill
                sizes="80px"
                className="object-cover"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center" aria-hidden="true">
                <svg className="h-10 w-10 text-gray-400" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z" />
                </svg>
              </div>
            )}
          </div>

          <div className="flex-1 text-center sm:text-left">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{profile.displayName}</h1>
            {profile.bio && (
              <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{profile.bio}</p>
            )}
            <p className="mt-1 text-xs text-gray-400 dark:text-gray-500">Member since {joinYear}</p>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{profile.recipeCount} public recipes</p>
          </div>

          {/* Add friend button — placeholder for task 025 */}
          <button
            aria-label="Add friend"
            className={clsx(
              'rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium',
              'text-gray-700 transition-colors hover:bg-gray-50',
              'dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700',
              'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
            )}
          >
            Add friend
          </button>
        </div>
      </div>

      {/* Public recipe list */}
      <div className="mt-8">
        <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Recipes</h2>
        {recipesLoading ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/3] animate-pulse rounded-xl bg-gray-100 dark:bg-gray-800" aria-hidden="true" />
            ))}
          </div>
        ) : recipes.length === 0 ? (
          <p className="text-sm text-gray-500 dark:text-gray-400">No public recipes yet.</p>
        ) : (
          <>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {recipes.map((recipe) => (
                <ProfileRecipeCard key={recipe.id} recipe={recipe} />
              ))}
            </div>
            {hasNextPage && (
              <div className="mt-6 flex justify-center">
                <button
                  onClick={() => fetchNextPage()}
                  disabled={isFetchingNextPage}
                  aria-label="Load more recipes"
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700 disabled:opacity-50"
                >
                  {isFetchingNextPage ? 'Loading…' : 'Load more'}
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
