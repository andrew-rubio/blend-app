'use client'

import Image from 'next/image'
import Link from 'next/link'
import type { Recipe } from '@/types/recipe'
import { LikeButton } from './LikeButton'
import { ShareButton } from './ShareButton'
import { CookButton } from './CookButton'

interface Props { recipe: Recipe }

export function RecipeHero({ recipe }: Props) {
  return (
    <section aria-label="Recipe overview">
      {recipe.imageUrl && (
        <div className="relative h-64 w-full overflow-hidden rounded-xl sm:h-80">
          <Image src={recipe.imageUrl} alt={recipe.title} fill className="object-cover" />
        </div>
      )}
      <div className="mt-4">
        <div className="flex flex-wrap gap-2">
          {recipe.cuisines.map((c) => (
            <span key={c} className="rounded-full bg-orange-100 px-3 py-1 text-xs font-medium text-orange-700">
              {c}
            </span>
          ))}
        </div>
        <h1 className="mt-2 text-2xl font-bold text-gray-900 sm:text-3xl">{recipe.title}</h1>
        <div className="mt-2 flex items-center gap-2 text-sm text-gray-500">
          {recipe.source === 'community' ? (
            <>
              {recipe.author.avatarUrl && (
                <Image src={recipe.author.avatarUrl} alt={recipe.author.name} width={24} height={24} className="rounded-full" />
              )}
              <Link href={`/profile/${recipe.author.id}`} className="font-medium text-gray-700 hover:underline">
                {recipe.author.name}
              </Link>
            </>
          ) : (
            <span className="font-medium text-gray-700">Spoonacular</span>
          )}
          <span>·</span>
          <time>{new Date(recipe.createdAt).toLocaleDateString()}</time>
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <LikeButton recipeId={recipe.id} liked={recipe.isLiked} likes={recipe.likes} />
          <ShareButton />
          <CookButton recipeId={recipe.id} />
        </div>
      </div>
    </section>
  )
}
