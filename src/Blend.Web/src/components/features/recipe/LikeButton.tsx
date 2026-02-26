'use client'

import { useToggleLike } from '@/lib/hooks/useRecipe'

interface Props {
  recipeId: string
  liked: boolean
  likes: number
}

export function LikeButton({ recipeId, liked, likes }: Props) {
  const { mutate, isPending } = useToggleLike(recipeId)
  return (
    <button
      aria-label={liked ? 'Unlike recipe' : 'Like recipe'}
      aria-pressed={liked}
      onClick={() => mutate({ liked })}
      disabled={isPending}
      className={`flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-medium transition-colors ${
        liked
          ? 'border-red-300 bg-red-50 text-red-600 hover:bg-red-100'
          : 'border-gray-300 bg-white text-gray-600 hover:bg-gray-50'
      } disabled:opacity-50`}
    >
      <span aria-hidden="true">{liked ? '♥' : '♡'}</span>
      <span>{likes}</span>
    </button>
  )
}
