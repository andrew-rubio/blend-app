import { Suspense } from 'react'
import type { Metadata } from 'next'
import { ExploreContainer } from '@/components/features/explore/ExploreContainer'

export const metadata: Metadata = {
  title: 'Explore',
}

export default function ExplorePage() {
  return (
    <Suspense fallback={<div className="mx-auto max-w-7xl px-4 py-8"><p className="text-gray-500 dark:text-gray-400">Loading…</p></div>}>
      <ExploreContainer />
    </Suspense>
  )
}
