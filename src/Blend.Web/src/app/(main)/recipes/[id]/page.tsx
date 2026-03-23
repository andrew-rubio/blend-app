import { Suspense } from 'react'
import type { Metadata } from 'next'
import { RecipeDetailContainer } from '@/components/features/recipe/RecipeDetailContainer'
import { RecipeDetailSkeleton } from '@/components/features/recipe/RecipeDetailSkeleton'

interface RecipePageProps {
  params: Promise<{ id: string }>
}

export async function generateMetadata({ params }: RecipePageProps): Promise<Metadata> {
  const { id } = await params
  return {
    title: `Recipe ${id}`,
  }
}

export default async function RecipePage({ params }: RecipePageProps) {
  const { id } = await params
  return (
    <Suspense fallback={<RecipeDetailSkeleton />}>
      <RecipeDetailContainer id={id} />
    </Suspense>
  )
}
