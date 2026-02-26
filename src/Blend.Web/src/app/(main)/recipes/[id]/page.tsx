import { RecipeDetailPage } from '@/components/features/recipe/RecipeDetailPage'

interface PageProps {
  params: Promise<{ id: string }>
}

export default async function RecipePage({ params }: PageProps) {
  const { id } = await params
  return <RecipeDetailPage id={id} />
}
