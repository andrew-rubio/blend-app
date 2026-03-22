'use client'

import { useState, useCallback } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/Button'

const SPLASH_SEEN_KEY = 'blend-intro-seen'

interface SplashStep {
  title: string
  description: string
  emoji: string
}

const STEPS: SplashStep[] = [
  {
    title: 'Discover Recipes',
    description: 'Explore thousands of recipes from cuisines around the world, tailored to your tastes and what\'s in your pantry.',
    emoji: '🍳',
  },
  {
    title: 'Create & Share',
    description: 'Build your own recipes, share them with the community, and get inspired by talented home chefs.',
    emoji: '📖',
  },
  {
    title: 'Connect with Chefs',
    description: 'Follow your favourite chefs, like recipes, and join a vibrant community of food lovers.',
    emoji: '👨‍🍳',
  },
  {
    title: 'Cook Mode',
    description: 'Follow step-by-step instructions hands-free with a distraction-free cooking experience.',
    emoji: '✨',
  },
]

export function useSplashIntro() {
  const [isVisible, setIsVisible] = useState(() => {
    if (typeof window === 'undefined') return false
    return !localStorage.getItem(SPLASH_SEEN_KEY)
  })

  const dismiss = useCallback(() => {
    if (typeof window !== 'undefined') {
      localStorage.setItem(SPLASH_SEEN_KEY, 'true')
    }
    setIsVisible(false)
  }, [])

  const show = useCallback(() => {
    setIsVisible(true)
  }, [])

  return { isVisible, dismiss, show }
}

interface SplashIntroProps {
  onDismiss?: () => void
}

export function SplashIntro({ onDismiss }: SplashIntroProps) {
  const [step, setStep] = useState(0)

  const handleDismiss = () => {
    if (typeof window !== 'undefined') {
      localStorage.setItem(SPLASH_SEEN_KEY, 'true')
    }
    onDismiss?.()
  }

  const handleNext = () => {
    if (step < STEPS.length - 1) {
      setStep((s) => s + 1)
    }
  }

  const handleBack = () => {
    if (step > 0) {
      setStep((s) => s - 1)
    }
  }

  const currentStep = STEPS[step]
  const isLastStep = step === STEPS.length - 1

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Welcome to Blend"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
      data-testid="splash-intro"
    >
      <div className="relative w-full max-w-md rounded-2xl bg-white p-8 shadow-2xl dark:bg-gray-900">
        <button
          onClick={handleDismiss}
          aria-label="Close welcome screen"
          className="absolute right-4 top-4 rounded-full p-1 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800 dark:hover:text-gray-300"
          data-testid="splash-close"
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <div className="text-center">
          <div className="mb-4 text-6xl" role="img" aria-label={currentStep.title}>
            {currentStep.emoji}
          </div>
          <h2 className="mb-3 text-2xl font-bold text-gray-900 dark:text-white">
            {currentStep.title}
          </h2>
          <p className="mb-8 text-gray-600 dark:text-gray-400">{currentStep.description}</p>
        </div>

        <div className="mb-6 flex justify-center gap-2" role="tablist" aria-label="Step indicators">
          {STEPS.map((_, i) => (
            <button
              key={i}
              role="tab"
              aria-selected={i === step}
              aria-label={`Step ${i + 1} of ${STEPS.length}`}
              onClick={() => setStep(i)}
              className={`h-2 rounded-full transition-all ${
                i === step
                  ? 'w-6 bg-primary-600'
                  : 'w-2 bg-gray-300 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500'
              }`}
            />
          ))}
        </div>

        <div className="flex gap-3">
          {step > 0 && (
            <Button variant="outline" className="flex-1" onClick={handleBack}>
              Back
            </Button>
          )}
          {!isLastStep ? (
            <Button variant="primary" className="flex-1" onClick={handleNext}>
              Next
            </Button>
          ) : (
            <div className="flex flex-1 flex-col gap-2">
              <Link href="/register" onClick={handleDismiss} className="block">
                <Button variant="primary" className="w-full" onClick={() => {}}>
                  Create an account
                </Button>
              </Link>
              <Link href="/login" onClick={handleDismiss} className="block">
                <Button variant="outline" className="w-full" onClick={() => {}}>
                  Sign in
                </Button>
              </Link>
              <Button variant="ghost" className="w-full" onClick={handleDismiss}>
                Continue as guest
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
