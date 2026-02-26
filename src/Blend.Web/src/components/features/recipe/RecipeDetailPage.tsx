'use client'

import { useRecipe } from '@/lib/hooks/useRecipe'
import { RecipeHero } from './RecipeHero'
import { RecipeTabs } from './RecipeTabs'
import { RecipeDetailSkeleton } from './RecipeDetailSkeleton'
import Link from 'next/link'

interface Props { id: string }

export function RecipeDetailPage({ id }: Props) {
  const { data: recipe, isLoading, error } = useRecipe(id)

  if (isLoading) return <RecipeDetailSkeleton />

  if (error) {
    const is404 = (error as { status?: number }).status === 404
    const is403 = (error as { status?: number }).status === 403
    return (
      <div className="flex min-h-screen flex-col items-center justify-center text-center px-4">
        <h1 className="text-2xl font-semibold text-gray-900">
          {is404 ? 'Recipe not found' : is403 ? 'This recipe is private' : 'Something went wrong'}
        </h1>
        <p className="mt-2 text-gray-500">
          {is404
            ? 'The recipe you are looking for does not exist.'
            : is403
              ? 'You do not have permission to view this recipe.'
              : 'Please try again later.'}
        </p>
        <Link href="/explore" className="mt-6 rounded-lg bg-orange-500 px-6 py-2 text-white hover:bg-orange-600">
          Back to Explore
        </Link>
      </div>
    )
  }

  if (!recipe) return null

  return (
    <main className="mx-auto max-w-4xl px-4 pb-16">
      <RecipeHero recipe={recipe} />
      <RecipeTabs recipe={recipe} />
    </main>
  )
}
