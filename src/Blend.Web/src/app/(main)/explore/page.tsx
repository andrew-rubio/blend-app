import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Explore',
}

export default function ExplorePage() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-6 text-3xl font-bold text-gray-900 dark:text-white">Explore Recipes</h1>
      <p className="text-gray-600 dark:text-gray-400">
        Discover thousands of recipes from our community of food lovers.
      </p>
    </div>
  )
}
