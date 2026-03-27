'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { GuestPromptModal, useGuestPrompt } from '@/components/features/GuestPromptModal'
import { OverviewTab } from './OverviewTab'
import { IngredientsTab } from './IngredientsTab'
import { DirectionsTab } from './DirectionsTab'
import { RecipeDetailSkeleton } from './RecipeDetailSkeleton'
import { useRecipe, useLikeRecipe } from '@/hooks/useRecipe'
import { useAuthStore } from '@/stores/authStore'
import type { Recipe } from '@/types'

type Tab = 'overview' | 'ingredients' | 'directions'

interface RecipeDetailContainerProps {
  id: string
}

export function RecipeDetailContainer({ id }: RecipeDetailContainerProps) {
  const { data: recipe, isLoading, error } = useRecipe(id)

  if (isLoading) {
    return <RecipeDetailSkeleton />
  }

  if (error) {
    const status = (error as { status?: number }).status
    if (status === 404) {
      return (
        <div className="mx-auto flex max-w-4xl flex-col items-center gap-4 px-4 py-16 text-center">
          <div className="text-5xl" role="img" aria-label="not found">
            🍽️
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Recipe not found</h1>
          <p className="text-gray-500 dark:text-gray-400">
            This recipe doesn&apos;t exist or may have been removed.
          </p>
          <Link href="/explore">
            <Button variant="primary">Back to Explore</Button>
          </Link>
        </div>
      )
    }
    if (status === 403) {
      return (
        <div className="mx-auto flex max-w-4xl flex-col items-center gap-4 px-4 py-16 text-center">
          <div className="text-5xl" role="img" aria-label="locked">
            🔒
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            This recipe is private
          </h1>
          <p className="text-gray-500 dark:text-gray-400">
            You don&apos;t have permission to view this recipe.
          </p>
          <Link href="/explore">
            <Button variant="primary">Back to Explore</Button>
          </Link>
        </div>
      )
    }
    return (
      <div className="mx-auto flex max-w-4xl flex-col items-center gap-4 px-4 py-16 text-center">
        <p className="text-gray-500 dark:text-gray-400">
          Something went wrong. Please try again later.
        </p>
        <Link href="/explore">
          <Button variant="primary">Back to Explore</Button>
        </Link>
      </div>
    )
  }

  if (!recipe) return null

  return <RecipeDetail recipe={recipe} />
}

function RecipeDetail({ recipe }: { recipe: Recipe }) {
  const [activeTab, setActiveTab] = useState<Tab>('overview')
  const [copied, setCopied] = useState(false)
  const { isAuthenticated } = useAuthStore()
  const { mutate: toggleLike, isPending: isLikePending } = useLikeRecipe()
  const { isOpen: isGuestOpen, message: guestMessage, prompt: promptGuest, close: closeGuest } =
    useGuestPrompt()
  const router = useRouter()

  const handleLike = () => {
    if (!isAuthenticated) {
      promptGuest('Sign in to like recipes and save your favourites.')
      return
    }
    toggleLike({ id: recipe.id, isCurrentlyLiked: recipe.isLiked ?? false })
  }

  const handleShare = async () => {
    if (typeof window === 'undefined') return
    const base = `${window.location.origin}${window.location.pathname}`
    const mobileUrl = `${base}?utm_source=share&utm_medium=mobile`
    const webUrl = `${base}?utm_source=share&utm_medium=web`
    try {
      if (navigator.share) {
        await navigator.share({ title: recipe.title, url: mobileUrl })
      } else {
        await navigator.clipboard.writeText(webUrl)
        setCopied(true)
        setTimeout(() => setCopied(false), 2000)
      }
    } catch {
      // User cancelled share or clipboard unavailable — ignore
    }
  }

  const handleCook = () => {
    if (!isAuthenticated) {
      promptGuest('Sign in to start cooking this dish.')
      return
    }
    router.push(`/cook/${recipe.id}`)
  }

  const tabs: { id: Tab; label: string }[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'ingredients', label: 'Ingredients' },
    { id: 'directions', label: 'Directions' },
  ]

  return (
    <div className="mx-auto max-w-4xl px-4 py-6">
      {/* Hero image */}
      {recipe.imageUrl && (
        <div className="aspect-video w-full overflow-hidden rounded-2xl bg-gray-100 dark:bg-gray-800">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={recipe.imageUrl}
            alt={recipe.title}
            className="h-full w-full object-cover"
          />
        </div>
      )}

      <div className="mt-6 flex flex-col gap-4">
        {/* Title & cuisine tags */}
        <div>
          <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl dark:text-white">
            {recipe.title}
          </h1>
          {recipe.cuisines.length > 0 && (
            <div className="mt-2 flex flex-wrap gap-2">
              {recipe.cuisines.map((c) => (
                <span
                  key={c}
                  className="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600 dark:bg-gray-800 dark:text-gray-300"
                >
                  {c}
                </span>
              ))}
            </div>
          )}
        </div>

        {/* Author info */}
        {recipe.dataSource === 'Community' && recipe.author ? (
          <Link
            href={`/users/${recipe.author.id}`}
            className="flex items-center gap-2 text-sm text-gray-600 hover:underline dark:text-gray-400"
            aria-label={`View profile of ${recipe.author.name}`}
          >
            {recipe.author.avatarUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={recipe.author.avatarUrl}
                alt={recipe.author.name}
                className="h-8 w-8 rounded-full object-cover"
              />
            ) : (
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary-100 text-primary-600 text-sm font-semibold dark:bg-primary-900 dark:text-primary-300">
                {recipe.author.name.charAt(0).toUpperCase()}
              </div>
            )}
            <span>{recipe.author.name}</span>
          </Link>
        ) : (
          <span
            className="inline-flex items-center gap-1 text-sm text-gray-500 dark:text-gray-400"
            aria-label="Source: Spoonacular"
          >
            <span>🥄</span>
            <span>Spoonacular</span>
          </span>
        )}

        {/* Actions */}
        <div className="flex flex-wrap gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={handleLike}
            disabled={isLikePending}
            aria-label={recipe.isLiked ? 'Unlike recipe' : 'Like recipe'}
            aria-pressed={recipe.isLiked ?? false}
          >
            {recipe.isLiked ? '❤️' : '🤍'} {recipe.likeCount}
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={handleShare}
            aria-label="Share recipe"
          >
            {copied ? 'Link copied!' : 'Share'}
          </Button>

          {isAuthenticated ? (
            <Button variant="primary" size="sm" onClick={handleCook}>
              Cook this dish
            </Button>
          ) : (
            <Button
              variant="outline"
              size="sm"
              onClick={handleCook}
              aria-label="Sign in to cook this dish"
            >
              Cook this dish
            </Button>
          )}
        </div>

        {/* Tab bar */}
        <div
          className="flex gap-1 overflow-x-auto border-b border-gray-200 dark:border-gray-700"
          role="tablist"
          aria-label="Recipe sections"
        >
          {tabs.map((tab) => (
            <button
              key={tab.id}
              role="tab"
              aria-selected={activeTab === tab.id}
              aria-controls={`tabpanel-${tab.id}`}
              id={`tab-${tab.id}`}
              onClick={() => setActiveTab(tab.id)}
              className={`whitespace-nowrap px-4 py-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 ${
                activeTab === tab.id
                  ? 'border-b-2 border-primary-600 text-primary-600 dark:text-primary-400'
                  : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Tab panels */}
        <div
          id={`tabpanel-overview`}
          role="tabpanel"
          aria-labelledby="tab-overview"
          hidden={activeTab !== 'overview'}
        >
          {activeTab === 'overview' && <OverviewTab recipe={recipe} />}
        </div>
        <div
          id={`tabpanel-ingredients`}
          role="tabpanel"
          aria-labelledby="tab-ingredients"
          hidden={activeTab !== 'ingredients'}
        >
          {activeTab === 'ingredients' && <IngredientsTab recipe={recipe} />}
        </div>
        <div
          id={`tabpanel-directions`}
          role="tabpanel"
          aria-labelledby="tab-directions"
          hidden={activeTab !== 'directions'}
        >
          {activeTab === 'directions' && <DirectionsTab recipe={recipe} />}
        </div>
      </div>

      {/* Guest prompt modal */}
      <GuestPromptModal
        isOpen={isGuestOpen}
        onClose={closeGuest}
        message={guestMessage}
      />
    </div>
  )
}
