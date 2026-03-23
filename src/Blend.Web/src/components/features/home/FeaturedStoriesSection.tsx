'use client'

import Image from 'next/image'
import { useRouter } from 'next/navigation'
import type { HomeFeaturedStory } from '@/types'

export interface FeaturedStoriesSectionProps {
  stories: HomeFeaturedStory[]
}

export function FeaturedStoriesSection({ stories }: FeaturedStoriesSectionProps) {
  const router = useRouter()

  if (stories.length === 0) return null

  return (
    <section aria-labelledby="stories-heading">
      <h2 id="stories-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
        Featured Stories
      </h2>
      <div
        className="flex gap-4 overflow-x-auto pb-2 scrollbar-hide"
        role="list"
        aria-label="Featured stories"
      >
        {stories.map((story) => (
          <div key={story.id} role="listitem" className="w-64 flex-shrink-0 sm:w-72">
            <StoryCard story={story} onClick={() => router.push(`/stories/${story.id}`)} />
          </div>
        ))}
      </div>
    </section>
  )
}

interface StoryCardProps {
  story: HomeFeaturedStory
  onClick: () => void
}

function StoryCard({ story, onClick }: StoryCardProps) {
  return (
    <article
      role="article"
      aria-label={story.title}
      onClick={onClick}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onClick()
        }
      }}
      tabIndex={0}
      className="group cursor-pointer overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md dark:border-gray-700 dark:bg-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
    >
      <div className="relative aspect-[16/9] w-full overflow-hidden bg-gray-100 dark:bg-gray-800">
        {story.coverImageUrl ? (
          <Image
            src={story.coverImageUrl}
            alt={story.title}
            fill
            sizes="(max-width: 768px) 80vw, 33vw"
            className="object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/20 dark:to-blue-800/20" aria-hidden="true">
            <svg className="h-10 w-10 text-blue-300 dark:text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
          </div>
        )}
      </div>
      <div className="p-3">
        <h3 className="line-clamp-2 text-sm font-semibold text-gray-900 dark:text-white">
          {story.title}
        </h3>
        {(story.author || story.readingTimeMinutes) && (
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            {story.author && <span>{story.author}</span>}
            {story.author && story.readingTimeMinutes && <span aria-hidden="true"> · </span>}
            {story.readingTimeMinutes && <span>{story.readingTimeMinutes} min read</span>}
          </p>
        )}
        {story.excerpt && (
          <p className="mt-1.5 line-clamp-2 text-xs text-gray-600 dark:text-gray-400">
            {story.excerpt}
          </p>
        )}
      </div>
    </article>
  )
}

export function FeaturedStoriesSkeleton() {
  return (
    <section aria-hidden="true">
      <div className="mb-3 h-6 w-36 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
      <div className="flex gap-4 overflow-x-hidden pb-2">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="w-64 flex-shrink-0 overflow-hidden rounded-xl border border-gray-200 dark:border-gray-700 sm:w-72">
            <div className="aspect-[16/9] w-full animate-pulse bg-gray-200 dark:bg-gray-800" />
            <div className="p-3 flex flex-col gap-2">
              <div className="h-4 w-3/4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
              <div className="h-3 w-1/2 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
