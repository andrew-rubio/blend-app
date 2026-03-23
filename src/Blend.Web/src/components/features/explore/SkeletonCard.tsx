'use client'

/**
 * Skeleton placeholder shown while recipe data is loading (EXPL-loading-states).
 */
export function SkeletonCard() {
  return (
    <div
      aria-hidden="true"
      className="flex flex-col overflow-hidden rounded-xl border border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-900"
    >
      {/* Image placeholder */}
      <div className="aspect-[4/3] w-full animate-pulse bg-gray-200 dark:bg-gray-800" />

      {/* Content */}
      <div className="flex flex-col gap-2 p-3">
        {/* Title */}
        <div className="h-4 w-3/4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        <div className="h-4 w-1/2 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />

        {/* Tags */}
        <div className="flex gap-1">
          <div className="h-5 w-16 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-5 w-12 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
        </div>

        {/* Meta */}
        <div className="flex justify-between pt-1">
          <div className="h-3 w-14 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-3 w-10 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      </div>
    </div>
  )
}

/** Renders n skeleton cards in a responsive grid. */
export function SkeletonGrid({ count = 8 }: { count?: number }) {
  return (
    <div
      className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4"
      aria-busy="true"
      aria-label="Loading recipes"
    >
      {Array.from({ length: count }).map((_, i) => (
        <SkeletonCard key={i} />
      ))}
    </div>
  )
}
