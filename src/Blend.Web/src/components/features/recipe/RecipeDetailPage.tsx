'use client'

import { useEffect } from 'react'
import Link from 'next/link'
import { useRecipe } from '@/hooks/useRecipe'
import { recordView } from '@/lib/api/recipes'
import { ApiError } from '@/lib/api/recipes'
import { RecipeHero } from './RecipeHero'
import { RecipeTabs } from './RecipeTabs'
import { LikeButton } from './LikeButton'
import { ShareButton } from './ShareButton'
import { CookButton } from './CookButton'
import { RecipeDetailSkeleton } from './RecipeDetailSkeleton'

interface RecipeDetailPageProps {
  id: string
}

export function RecipeDetailPage({ id }: RecipeDetailPageProps) {
  const { data: recipe, isLoading, error } = useRecipe(id)

  useEffect(() => {
    if (recipe) {
      recordView(id).catch(() => {})
    }
  }, [recipe, id])

  if (isLoading) return <RecipeDetailSkeleton />

  if (error) {
    if (error instanceof ApiError) {
      if (error.status === 404) {
        return (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <h2 className="text-2xl font-semibold text-gray-800">Recipe not found</h2>
            <p className="mt-2 text-gray-500">
              This recipe doesn&apos;t exist or has been removed.
            </p>
            <Link
              href="/explore"
              className="mt-6 rounded-lg bg-orange-500 px-6 py-2 text-white hover:bg-orange-600"
            >
              Back to Explore
            </Link>
          </div>
        )
      }
      if (error.status === 403) {
        return (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <h2 className="text-2xl font-semibold text-gray-800">This recipe is private</h2>
            <p className="mt-2 text-gray-500">You don&apos;t have permission to view this recipe.</p>
            <Link
              href="/explore"
              className="mt-6 rounded-lg bg-orange-500 px-6 py-2 text-white hover:bg-orange-600"
            >
              Back to Explore
            </Link>
          </div>
        )
      }
    }
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <h2 className="text-2xl font-semibold text-gray-800">Something went wrong</h2>
        <p className="mt-2 text-gray-500">Please try again later.</p>
      </div>
    )
  }

  if (!recipe) return null

  return (
    <div className="min-h-screen bg-white">
      <RecipeHero recipe={recipe} />
      <div className="mx-auto max-w-4xl px-4 py-6">
        <div className="mb-6 flex flex-wrap items-center gap-3">
          <LikeButton
            recipeId={recipe.id}
            initialLikes={recipe.likes}
            initialIsLiked={recipe.isLiked}
          />
          <ShareButton />
          <CookButton recipeId={recipe.id} />
        </div>
        <RecipeTabs recipe={recipe} />
      </div>
    </div>
  )
}
