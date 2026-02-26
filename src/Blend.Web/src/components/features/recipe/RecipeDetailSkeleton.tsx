export function RecipeDetailSkeleton() {
  return (
    <div className="mx-auto max-w-4xl animate-pulse px-4 pb-16" aria-label="Loading recipe">
      <div className="h-64 w-full rounded-xl bg-gray-200 sm:h-80" />
      <div className="mt-4 space-y-3">
        <div className="h-4 w-24 rounded bg-gray-200" />
        <div className="h-8 w-3/4 rounded bg-gray-200" />
        <div className="h-4 w-48 rounded bg-gray-200" />
      </div>
      <div className="mt-6 flex gap-2">
        <div className="h-8 w-24 rounded-full bg-gray-200" />
        <div className="h-8 w-24 rounded-full bg-gray-200" />
        <div className="h-8 w-32 rounded-full bg-gray-200" />
      </div>
      <div className="mt-8 space-y-4">
        <div className="h-4 rounded bg-gray-200" />
        <div className="h-4 w-5/6 rounded bg-gray-200" />
        <div className="h-4 w-4/6 rounded bg-gray-200" />
      </div>
    </div>
  )
}
