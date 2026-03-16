import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Home',
}

export default function MainHomePage() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-6 text-3xl font-bold text-gray-900 dark:text-white">Your Feed</h1>
      <p className="text-gray-600 dark:text-gray-400">
        Welcome to your personalized recipe feed. Sign in to see recipes from people you follow.
      </p>
    </div>
  )
}
