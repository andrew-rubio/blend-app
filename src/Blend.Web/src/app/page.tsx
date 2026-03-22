'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/Button'
import { SplashIntro } from '@/components/features/SplashIntro'

const SPLASH_SEEN_KEY = 'blend-intro-seen'

export default function HomePage() {
  const [showSplash, setShowSplash] = useState(false)

  useEffect(() => {
    const seen = localStorage.getItem(SPLASH_SEEN_KEY)
    if (!seen) {
      setShowSplash(true)
    }
  }, [])

  return (
    <>
      {showSplash && <SplashIntro onDismiss={() => setShowSplash(false)} />}

      <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-b from-white to-gray-50 dark:from-gray-950 dark:to-gray-900">
        <div className="mx-auto max-w-4xl px-4 text-center">
          <h1 className="mb-6 text-5xl font-bold tracking-tight text-gray-900 dark:text-white">
            Welcome to{' '}
            <span className="text-primary-600">Blend</span>
          </h1>
          <p className="mb-8 text-xl text-gray-600 dark:text-gray-400">
            Discover, create, and share amazing recipes with a community of food lovers.
          </p>
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
            <Link href="/register">
              <Button variant="primary" size="lg">
                Get started for free
              </Button>
            </Link>
            <Link href="/explore">
              <Button variant="outline" size="lg">
                Explore recipes
              </Button>
            </Link>
          </div>

          <div className="mt-16 grid grid-cols-1 gap-8 sm:grid-cols-3">
            {[
              { title: 'Discover', description: 'Find thousands of recipes from around the world' },
              { title: 'Create', description: 'Share your culinary creations with the community' },
              { title: 'Connect', description: 'Follow chefs and get inspired daily' },
            ].map((feature) => (
              <div
                key={feature.title}
                className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900"
              >
                <h3 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">
                  {feature.title}
                </h3>
                <p className="text-gray-600 dark:text-gray-400">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </>
  )
}
