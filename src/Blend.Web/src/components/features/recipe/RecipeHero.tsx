import Image from 'next/image'
import { Badge } from '@/components/ui/Badge'
import type { Recipe } from '@/types/recipe'

interface RecipeHeroProps {
  recipe: Recipe
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

export function RecipeHero({ recipe }: RecipeHeroProps) {
  return (
    <div className="relative">
      {recipe.imageUrl && (
        <div className="relative h-64 w-full sm:h-80 md:h-96">
          <Image
            src={recipe.imageUrl}
            alt={recipe.title}
            fill
            className="object-cover"
            priority
          />
        </div>
      )}
      <div className="mx-auto max-w-4xl px-4 py-6">
        <h1 className="text-3xl font-bold text-gray-900 sm:text-4xl">{recipe.title}</h1>

        {recipe.cuisines.length > 0 && (
          <div className="mt-3 flex flex-wrap gap-2">
            {recipe.cuisines.map((cuisine) => (
              <Badge key={cuisine} variant="cuisine">
                {cuisine}
              </Badge>
            ))}
          </div>
        )}

        <div className="mt-4 flex items-center gap-3">
          {recipe.source === 'community' ? (
            <>
              {recipe.author.avatarUrl && (
                <Image
                  src={recipe.author.avatarUrl}
                  alt={recipe.author.name}
                  width={32}
                  height={32}
                  className="rounded-full"
                />
              )}
              <span className="text-sm text-gray-600">By {recipe.author.name}</span>
            </>
          ) : (
            <span className="text-sm font-medium text-gray-600">
              Powered by{' '}
              <span className="text-orange-500">Spoonacular</span>
            </span>
          )}
        </div>

        <div className="mt-2 flex gap-4 text-xs text-gray-500">
          <span>Created {formatDate(recipe.createdAt)}</span>
          <span>Updated {formatDate(recipe.updatedAt)}</span>
        </div>
      </div>
    </div>
  )
}
