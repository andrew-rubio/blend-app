'use client'

import { useState } from 'react'
import Image from 'next/image'
import type { HomeFeaturedVideo } from '@/types'

export interface FeaturedVideosSectionProps {
  videos: HomeFeaturedVideo[]
}

export function FeaturedVideosSection({ videos }: FeaturedVideosSectionProps) {
  const [activeVideo, setActiveVideo] = useState<HomeFeaturedVideo | null>(null)

  if (videos.length === 0) return null

  return (
    <section aria-labelledby="videos-heading">
      <h2 id="videos-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
        Featured Videos
      </h2>
      <div
        className="flex gap-4 overflow-x-auto pb-2 scrollbar-hide"
        role="list"
        aria-label="Featured videos"
      >
        {videos.map((video) => (
          <div key={video.id} role="listitem" className="w-56 flex-shrink-0 sm:w-64">
            <VideoCard video={video} onPlay={() => setActiveVideo(video)} />
          </div>
        ))}
      </div>

      {activeVideo && (
        <VideoPlayer video={activeVideo} onClose={() => setActiveVideo(null)} />
      )}
    </section>
  )
}

interface VideoCardProps {
  video: HomeFeaturedVideo
  onPlay: () => void
}

function VideoCard({ video, onPlay }: VideoCardProps) {
  return (
    <article
      role="article"
      aria-label={`Play video: ${video.title}`}
      onClick={onPlay}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onPlay()
        }
      }}
      tabIndex={0}
      className="group cursor-pointer overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md dark:border-gray-700 dark:bg-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
    >
      <div className="relative aspect-video w-full overflow-hidden bg-gray-900 dark:bg-black">
        {video.thumbnailUrl ? (
          <Image
            src={video.thumbnailUrl}
            alt={video.title}
            fill
            sizes="(max-width: 768px) 70vw, 33vw"
            className="object-cover transition-opacity duration-300 group-hover:opacity-80"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gray-900" aria-hidden="true" />
        )}
        {/* Play button overlay */}
        <div className="absolute inset-0 flex items-center justify-center" aria-hidden="true">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white/90 shadow-lg transition-transform duration-200 group-hover:scale-110">
            <svg className="ml-1 h-5 w-5 text-gray-900" fill="currentColor" viewBox="0 0 24 24">
              <path d="M8 5v14l11-7z" />
            </svg>
          </div>
        </div>
      </div>
      <div className="p-3">
        <h3 className="line-clamp-2 text-sm font-semibold text-gray-900 dark:text-white">
          {video.title}
        </h3>
        {video.creator && (
          <p className="mt-0.5 text-xs text-gray-500 dark:text-gray-400">{video.creator}</p>
        )}
      </div>
    </article>
  )
}

interface VideoPlayerProps {
  video: HomeFeaturedVideo
  onClose: () => void
}

function VideoPlayer({ video, onClose }: VideoPlayerProps) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/80"
      role="dialog"
      aria-modal="true"
      aria-label={`Video player: ${video.title}`}
      onClick={onClose}
    >
      <div
        className="relative mx-4 w-full max-w-3xl overflow-hidden rounded-2xl bg-black"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          onClick={onClose}
          aria-label="Close video player"
          className="absolute right-3 top-3 z-10 flex h-8 w-8 items-center justify-center rounded-full bg-black/60 text-white hover:bg-black/80 transition-colors"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <div className="aspect-video w-full">
          {video.videoUrl ? (
            <video
              src={video.videoUrl}
              controls
              autoPlay
              className="h-full w-full"
              aria-label={video.title}
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center bg-gray-900 text-gray-400">
              <p className="text-sm">Video unavailable</p>
            </div>
          )}
        </div>
        <div className="px-4 py-3">
          <h3 className="font-semibold text-white">{video.title}</h3>
          {video.creator && <p className="mt-0.5 text-sm text-gray-400">{video.creator}</p>}
        </div>
      </div>
    </div>
  )
}

export function FeaturedVideosSkeleton() {
  return (
    <section aria-hidden="true">
      <div className="mb-3 h-6 w-36 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
      <div className="flex gap-4 overflow-x-hidden pb-2">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="w-56 flex-shrink-0 overflow-hidden rounded-xl border border-gray-200 dark:border-gray-700 sm:w-64">
            <div className="aspect-video w-full animate-pulse bg-gray-200 dark:bg-gray-800" />
            <div className="p-3 flex flex-col gap-1.5">
              <div className="h-4 w-3/4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
              <div className="h-3 w-1/2 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
