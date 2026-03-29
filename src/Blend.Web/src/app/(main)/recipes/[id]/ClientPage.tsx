'use client'

import { Suspense } from 'react'
import { useParams } from 'next/navigation'
import { RecipeDetailContainer } from '@/components/features/recipe/RecipeDetailContainer'
import { RecipeDetailSkeleton } from '@/components/features/recipe/RecipeDetailSkeleton'

export default function RecipeClientPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <Suspense fallback={<RecipeDetailSkeleton />}>
      <RecipeDetailContainer id={id} />
    </Suspense>
  )
}
