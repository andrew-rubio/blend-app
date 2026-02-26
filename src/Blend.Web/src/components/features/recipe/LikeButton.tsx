'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/Button'
import { toggleLike } from '@/lib/api/recipes'

interface LikeButtonProps {
  recipeId: string
  initialLikes: number
  initialIsLiked: boolean
}

export function LikeButton({ recipeId, initialLikes, initialIsLiked }: LikeButtonProps) {
  const [isLiked, setIsLiked] = useState(initialIsLiked)
  const [likes, setLikes] = useState(initialLikes)

  const handleLike = async () => {
    const prevLiked = isLiked
    const prevLikes = likes

    // Optimistic update
    setIsLiked(!prevLiked)
    setLikes(prevLiked ? likes - 1 : likes + 1)

    try {
      await toggleLike(recipeId, !prevLiked)
    } catch {
      // Rollback on error
      setIsLiked(prevLiked)
      setLikes(prevLikes)
    }
  }

  return (
    <Button
      variant={isLiked ? 'primary' : 'secondary'}
      size="sm"
      onClick={handleLike}
      aria-label={isLiked ? 'Unlike recipe' : 'Like recipe'}
      aria-pressed={isLiked}
    >
      {isLiked ? '❤️' : '🤍'} {likes}
    </Button>
  )
}
