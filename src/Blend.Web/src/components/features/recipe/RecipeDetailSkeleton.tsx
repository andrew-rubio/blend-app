'use client'

export function RecipeDetailSkeleton() {
  return (
    <div className="mx-auto max-w-4xl animate-pulse px-4 py-8" aria-label="Loading recipe" aria-busy="true">
      {/* Hero image */}
      <div className="aspect-video w-full rounded-2xl bg-gray-200 dark:bg-gray-700" />

      <div className="mt-6 flex flex-col gap-4">
        {/* Title */}
        <div className="h-8 w-3/4 rounded-lg bg-gray-200 dark:bg-gray-700" />

        {/* Tags row */}
        <div className="flex gap-2">
          <div className="h-6 w-20 rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-6 w-16 rounded-full bg-gray-200 dark:bg-gray-700" />
        </div>

        {/* Author row */}
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-4 w-32 rounded bg-gray-200 dark:bg-gray-700" />
        </div>

        {/* Action buttons */}
        <div className="flex gap-3">
          <div className="h-10 w-24 rounded-lg bg-gray-200 dark:bg-gray-700" />
          <div className="h-10 w-24 rounded-lg bg-gray-200 dark:bg-gray-700" />
          <div className="h-10 w-36 rounded-lg bg-gray-200 dark:bg-gray-700" />
        </div>

        {/* Tab bar */}
        <div className="flex gap-4 border-b border-gray-200 pb-2 dark:border-gray-700">
          <div className="h-8 w-24 rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-8 w-28 rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-8 w-24 rounded bg-gray-200 dark:bg-gray-700" />
        </div>

        {/* Content area */}
        <div className="flex flex-col gap-3 pt-2">
          <div className="h-4 w-full rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-4 w-5/6 rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-4 w-4/6 rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      </div>
    </div>
  )
}
