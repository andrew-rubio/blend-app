'use client'

import { useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { clsx } from 'clsx'
import { ProfileRecipeCard } from './ProfileRecipeCard'
import { DeleteConfirmDialog } from './DeleteConfirmDialog'
import { useMyRecipes, useLikedRecipes, useToggleRecipeVisibility, useDeleteRecipe } from '@/hooks/useProfile'
import type { ProfileRecipe } from '@/types'

type TabId = 'my-recipes' | 'liked-recipes'

export function ProfileRecipeTabs() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState<TabId>('my-recipes')
  const [pendingDeleteRecipe, setPendingDeleteRecipe] = useState<ProfileRecipe | null>(null)

  const {
    data: myRecipesData,
    isLoading: myRecipesLoading,
    fetchNextPage: fetchMoreMyRecipes,
    hasNextPage: hasMoreMyRecipes,
    isFetchingNextPage: isFetchingMoreMyRecipes,
  } = useMyRecipes()

  const {
    data: likedRecipesData,
    isLoading: likedRecipesLoading,
    fetchNextPage: fetchMoreLiked,
    hasNextPage: hasMoreLiked,
    isFetchingNextPage: isFetchingMoreLiked,
  } = useLikedRecipes()

  const { mutate: toggleVisibility } = useToggleRecipeVisibility()
  const { mutate: deleteRecipe, isPending: isDeleting } = useDeleteRecipe()

  const myRecipes = myRecipesData?.pages.flatMap((p) => p.recipes) ?? []
  const likedRecipes = likedRecipesData?.pages.flatMap((p) => p.recipes) ?? []

  const handleEdit = useCallback((id: string) => {
    router.push(`/recipes/${id}/edit`)
  }, [router])

  const handleToggleVisibility = useCallback((id: string, currentIsPublic: boolean) => {
    toggleVisibility({ recipeId: id, isPublic: !currentIsPublic })
  }, [toggleVisibility])

  const handleDeleteClick = useCallback((id: string) => {
    const recipe = myRecipes.find((r) => r.id === id)
    if (recipe) setPendingDeleteRecipe(recipe)
  }, [myRecipes])

  const handleConfirmDelete = useCallback(() => {
    if (!pendingDeleteRecipe) return
    deleteRecipe(pendingDeleteRecipe.id, {
      onSuccess: () => setPendingDeleteRecipe(null),
    })
  }, [pendingDeleteRecipe, deleteRecipe])

  const tabs: { id: TabId; label: string }[] = [
    { id: 'my-recipes', label: 'My Recipes' },
    { id: 'liked-recipes', label: 'Liked Recipes' },
  ]

  return (
    <div className="mt-6">
      {/* Tab bar */}
      <div role="tablist" aria-label="Recipe tabs" className="flex border-b border-gray-200 dark:border-gray-700">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            role="tab"
            aria-selected={activeTab === tab.id}
            aria-controls={`tabpanel-${tab.id}`}
            id={`tab-${tab.id}`}
            onClick={() => setActiveTab(tab.id)}
            className={clsx(
              'px-4 py-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500',
              activeTab === tab.id
                ? 'border-b-2 border-primary-600 text-primary-600 dark:text-primary-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* My Recipes panel */}
      <div
        role="tabpanel"
        id="tabpanel-my-recipes"
        aria-labelledby="tab-my-recipes"
        hidden={activeTab !== 'my-recipes'}
        className="mt-4"
      >
        {myRecipesLoading ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/3] animate-pulse rounded-xl bg-gray-100 dark:bg-gray-800" aria-hidden="true" />
            ))}
          </div>
        ) : myRecipes.length === 0 ? (
          <div className="flex flex-col items-center gap-3 py-12 text-center">
            <svg className="h-12 w-12 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
            <p className="text-sm font-medium text-gray-900 dark:text-white">No recipes yet</p>
            <p className="text-xs text-gray-500 dark:text-gray-400">Start cooking to create your first recipe!</p>
            <button
              onClick={() => router.push('/cook')}
              aria-label="Start cooking"
              className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
            >
              Start cooking
            </button>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {myRecipes.map((recipe) => (
                <ProfileRecipeCard
                  key={recipe.id}
                  recipe={recipe}
                  showContextMenu
                  onEdit={handleEdit}
                  onToggleVisibility={handleToggleVisibility}
                  onDelete={handleDeleteClick}
                />
              ))}
            </div>
            {hasMoreMyRecipes && (
              <div className="mt-6 flex justify-center">
                <button
                  onClick={() => fetchMoreMyRecipes()}
                  disabled={isFetchingMoreMyRecipes}
                  aria-label="Load more recipes"
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700 disabled:opacity-50"
                >
                  {isFetchingMoreMyRecipes ? 'Loading…' : 'Load more'}
                </button>
              </div>
            )}
          </>
        )}
      </div>

      {/* Liked Recipes panel */}
      <div
        role="tabpanel"
        id="tabpanel-liked-recipes"
        aria-labelledby="tab-liked-recipes"
        hidden={activeTab !== 'liked-recipes'}
        className="mt-4"
      >
        {likedRecipesLoading ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/3] animate-pulse rounded-xl bg-gray-100 dark:bg-gray-800" aria-hidden="true" />
            ))}
          </div>
        ) : likedRecipes.length === 0 ? (
          <div className="flex flex-col items-center gap-3 py-12 text-center">
            <svg className="h-12 w-12 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
            <p className="text-sm font-medium text-gray-900 dark:text-white">No liked recipes</p>
            <p className="text-xs text-gray-500 dark:text-gray-400">Explore recipes and like the ones you love!</p>
            <button
              onClick={() => router.push('/explore')}
              aria-label="Explore recipes"
              className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
            >
              Explore recipes
            </button>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {likedRecipes.map((recipe) => (
                <ProfileRecipeCard key={recipe.id} recipe={recipe} />
              ))}
            </div>
            {hasMoreLiked && (
              <div className="mt-6 flex justify-center">
                <button
                  onClick={() => fetchMoreLiked()}
                  disabled={isFetchingMoreLiked}
                  aria-label="Load more liked recipes"
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700 disabled:opacity-50"
                >
                  {isFetchingMoreLiked ? 'Loading…' : 'Load more'}
                </button>
              </div>
            )}
          </>
        )}
      </div>

      {/* Delete confirmation dialog */}
      {pendingDeleteRecipe && (
        <DeleteConfirmDialog
          title={pendingDeleteRecipe.title}
          isDeleting={isDeleting}
          onConfirm={handleConfirmDelete}
          onCancel={() => setPendingDeleteRecipe(null)}
        />
      )}
    </div>
  )
}
