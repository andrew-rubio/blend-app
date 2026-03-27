import { Suspense } from 'react'
import type { Metadata } from 'next'
import { notFound } from 'next/navigation'
import { RecipeDetailContainer } from '@/components/features/recipe/RecipeDetailContainer'
import { RecipeDetailSkeleton } from '@/components/features/recipe/RecipeDetailSkeleton'
import {
  fetchRecipeForMetadata,
  buildRecipeMetadata,
  buildRecipeJsonLd,
  safeJsonStringify,
} from '@/lib/metadata'

interface RecipePageProps {
  params: Promise<{ id: string }>
}

/** Throw a Next.js 404 for missing or private recipes; re-throw everything else. */
function handleRecipeFetchError(err: unknown): never {
  const status = (err as { status?: number }).status
  if (status === 404 || status === 403) {
    notFound()
  }
  throw err
}

export async function generateMetadata({ params }: RecipePageProps): Promise<Metadata> {
  const { id } = await params
  try {
    const recipe = await fetchRecipeForMetadata(id)
    return buildRecipeMetadata(recipe, id)
  } catch (err) {
    handleRecipeFetchError(err)
  }
}

export default async function RecipePage({ params }: RecipePageProps) {
  const { id } = await params

  let jsonLd: Record<string, unknown> | null = null
  try {
    const recipe = await fetchRecipeForMetadata(id)
    jsonLd = buildRecipeJsonLd(recipe, id)
  } catch (err) {
    handleRecipeFetchError(err)
  }

  return (
    <>
      {jsonLd && (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: safeJsonStringify(jsonLd) }}
        />
      )}
      <Suspense fallback={<RecipeDetailSkeleton />}>
        <RecipeDetailContainer id={id} />
      </Suspense>
    </>
  )
}
