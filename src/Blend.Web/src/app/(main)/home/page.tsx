import { Suspense } from 'react'
import type { Metadata } from 'next'
import { HomeContainer } from '@/components/features/home/HomeContainer'

export const metadata: Metadata = {
  title: 'Home',
}

export default function MainHomePage() {
  return (
    <Suspense fallback={<div className="mx-auto max-w-7xl px-4 py-8"><p className="text-gray-500 dark:text-gray-400">Loading…</p></div>}>
      <HomeContainer />
    </Suspense>
  )
}
