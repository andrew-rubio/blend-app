'use client'

import { useState, useEffect, useCallback, useRef } from 'react'
import Image from 'next/image'
import { useRouter } from 'next/navigation'
import { clsx } from 'clsx'
import type { HomeFeaturedRecipe } from '@/types'

export interface FeaturedCarouselProps {
  recipes: HomeFeaturedRecipe[]
  autoAdvanceInterval?: number
}

export function FeaturedCarousel({ recipes, autoAdvanceInterval = 4000 }: FeaturedCarouselProps) {
  const router = useRouter()
  const [activeIndex, setActiveIndex] = useState(0)
  const [isPaused, setIsPaused] = useState(false)
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const pauseTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const advance = useCallback(() => {
    setActiveIndex((i) => (i + 1) % recipes.length)
  }, [recipes.length])

  const goTo = useCallback((index: number) => {
    setActiveIndex(index)
    setIsPaused(true)
    if (pauseTimerRef.current) clearTimeout(pauseTimerRef.current)
    pauseTimerRef.current = setTimeout(() => setIsPaused(false), autoAdvanceInterval)
  }, [autoAdvanceInterval])

  useEffect(() => {
    return () => {
      if (pauseTimerRef.current) clearTimeout(pauseTimerRef.current)
    }
  }, [])

  useEffect(() => {
    if (recipes.length <= 1 || isPaused) {
      if (timerRef.current) clearInterval(timerRef.current)
      return
    }
    timerRef.current = setInterval(advance, autoAdvanceInterval)
    return () => {
      if (timerRef.current) clearInterval(timerRef.current)
    }
  }, [advance, autoAdvanceInterval, isPaused, recipes.length])

  if (recipes.length === 0) return null

  const current = recipes[activeIndex]

  return (
    <section
      aria-label="Featured recipes"
      className="relative w-full overflow-hidden rounded-2xl"
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
    >
      {/* Card */}
      <div
        role="button"
        tabIndex={0}
        aria-label={`View recipe: ${current.title}`}
        onClick={() => router.push(`/recipes/${current.id}`)}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault()
            router.push(`/recipes/${current.id}`)
          }
        }}
        className="group relative block h-56 w-full cursor-pointer overflow-hidden rounded-2xl bg-gray-200 sm:h-72 lg:h-80"
      >
        {current.imageUrl ? (
          <Image
            src={current.imageUrl}
            alt={current.title}
            fill
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 80vw, 70vw"
            className="object-cover transition-transform duration-500 group-hover:scale-105"
            priority
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-orange-100 to-orange-200 dark:from-orange-900/30 dark:to-orange-800/30" aria-hidden="true">
            <svg className="h-16 w-16 text-orange-300 dark:text-orange-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}
        {/* Gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/20 to-transparent" aria-hidden="true" />
        {/* Text overlay */}
        <div className="absolute bottom-0 left-0 right-0 p-4 sm:p-6">
          <p className="mb-1 text-xs font-medium uppercase tracking-wider text-orange-300">
            Featured Recipe
          </p>
          <h3 className="text-lg font-bold text-white sm:text-xl lg:text-2xl line-clamp-2">
            {current.title}
          </h3>
          {current.attribution && (
            <p className="mt-1 text-sm text-gray-300">by {current.attribution}</p>
          )}
        </div>
      </div>

      {/* Dot indicators */}
      {recipes.length > 1 && (
        <div
          className="mt-3 flex items-center justify-center gap-1.5"
          role="tablist"
          aria-label="Carousel navigation"
        >
          {recipes.map((recipe, i) => (
            <button
              key={recipe.id}
              role="tab"
              aria-selected={i === activeIndex}
              aria-label={`Go to slide ${i + 1}: ${recipe.title}`}
              onClick={() => goTo(i)}
              className={clsx(
                'h-2 rounded-full transition-all duration-300',
                i === activeIndex
                  ? 'w-6 bg-orange-500'
                  : 'w-2 bg-gray-300 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500'
              )}
            />
          ))}
        </div>
      )}
    </section>
  )
}

export function FeaturedCarouselSkeleton() {
  return (
    <div aria-hidden="true" className="w-full">
      <div className="h-56 w-full animate-pulse rounded-2xl bg-gray-200 dark:bg-gray-800 sm:h-72 lg:h-80" />
      <div className="mt-3 flex items-center justify-center gap-1.5">
        {[0, 1, 2].map((i) => (
          <div key={i} className="h-2 w-2 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
        ))}
      </div>
    </div>
  )
}
