import { Skeleton } from '@/components/ui/Skeleton'

export function RecipeDetailSkeleton() {
  return (
    <div className="min-h-screen bg-white" aria-busy="true" aria-label="Loading recipe">
      <Skeleton className="h-64 w-full sm:h-80 md:h-96" />
      <div className="mx-auto max-w-4xl px-4 py-6 space-y-4">
        <Skeleton className="h-10 w-3/4" />
        <div className="flex gap-2">
          <Skeleton className="h-6 w-20" />
          <Skeleton className="h-6 w-24" />
        </div>
        <Skeleton className="h-4 w-48" />
        <div className="flex gap-3 mt-6">
          <Skeleton className="h-10 w-24" />
          <Skeleton className="h-10 w-24" />
          <Skeleton className="h-10 w-36" />
        </div>
      </div>
    </div>
  )
}
