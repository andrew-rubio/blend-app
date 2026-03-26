'use client'

import { useState } from 'react'
import Link from 'next/link'
import { clsx } from 'clsx'
import { useUserPreferences } from '@/hooks/usePreferences'
import { useAppSettings } from '@/hooks/useSettings'
import { UnitToggle } from './UnitToggle'
import { IngredientCatalogue } from './IngredientCatalogue'
import { IngredientSubmissionForm } from './IngredientSubmissionForm'
import { MySubmissions } from './MySubmissions'
import { AccountDeletionWizard } from './AccountDeletionWizard'
import { DeletionCancellationBanner } from './DeletionCancellationBanner'
import { SplashIntro } from '@/components/features/SplashIntro'

const SPLASH_SEEN_KEY = 'blend-intro-seen'

type PanelView =
  | null
  | 'ingredient-catalogue'
  | 'ingredient-submit'
  | 'my-submissions'

/**
 * Main settings page container (SETT-01 through SETT-24).
 * Groups settings into sections: Preferences, Ingredients, Units, App, Account.
 */
export function SettingsContainer() {
  const [activePanel, setActivePanel] = useState<PanelView>(null)
  const [showDeletionWizard, setShowDeletionWizard] = useState(false)
  const [showSplash, setShowSplash] = useState(false)

  // Load server state
  useAppSettings()

  const { data: preferences } = useUserPreferences()

  const preferenceSummaryParts: string[] = []
  if (preferences) {
    if (preferences.favoriteCuisines.length > 0) {
      preferenceSummaryParts.push(`${preferences.favoriteCuisines.length} cuisine${preferences.favoriteCuisines.length !== 1 ? 's' : ''}`)
    }
    if (preferences.diets.length > 0) {
      preferenceSummaryParts.push(`${preferences.diets.length} diet${preferences.diets.length !== 1 ? 's' : ''}`)
    }
    if (preferences.intolerances.length > 0) {
      preferenceSummaryParts.push(`${preferences.intolerances.length} intolerance${preferences.intolerances.length !== 1 ? 's' : ''}`)
    }
  }
  const preferenceSummary = preferenceSummaryParts.length > 0 ? preferenceSummaryParts.join(', ') : null

  function handleShare() {
    const shareData = {
      title: 'Blend – Recipe & Cooking App',
      text: 'Discover, create, and share amazing recipes on Blend!',
      url: typeof window !== 'undefined' ? window.location.origin : 'https://blend.app',
    }
    if (typeof navigator !== 'undefined' && navigator.share) {
      void navigator.share(shareData)
    } else if (typeof navigator !== 'undefined') {
      void navigator.clipboard.writeText(shareData.url)
    }
  }

  function handleReplaySplash() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(SPLASH_SEEN_KEY)
    }
    setShowSplash(true)
  }

  if (activePanel === 'ingredient-catalogue') {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <button
          type="button"
          onClick={() => setActivePanel(null)}
          className="mb-6 flex items-center gap-1 text-sm text-primary-600 hover:underline focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Back to settings"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Settings
        </button>
        <h2 className="mb-4 text-xl font-bold text-gray-900 dark:text-white">Ingredient Catalogue</h2>
        <IngredientCatalogue />
      </div>
    )
  }

  if (activePanel === 'ingredient-submit') {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <button
          type="button"
          onClick={() => setActivePanel(null)}
          className="mb-6 flex items-center gap-1 text-sm text-primary-600 hover:underline focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Back to settings"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Settings
        </button>
        <h2 className="mb-4 text-xl font-bold text-gray-900 dark:text-white">Submit New Ingredient</h2>
        <IngredientSubmissionForm onClose={() => setActivePanel(null)} />
      </div>
    )
  }

  if (activePanel === 'my-submissions') {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <button
          type="button"
          onClick={() => setActivePanel(null)}
          className="mb-6 flex items-center gap-1 text-sm text-primary-600 hover:underline focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Back to settings"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Settings
        </button>
        <h2 className="mb-4 text-xl font-bold text-gray-900 dark:text-white">My Submissions</h2>
        <MySubmissions />
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      {showSplash && <SplashIntro onDismiss={() => setShowSplash(false)} />}
      {showDeletionWizard && (
        <AccountDeletionWizard onClose={() => setShowDeletionWizard(false)} />
      )}

      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">App Settings</h1>

      <DeletionCancellationBanner />

      {/* ── Preferences ─────────────────────────────────────────────────────── */}
      <section aria-labelledby="preferences-heading" className="mt-6">
        <h2 id="preferences-heading" className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Preferences
        </h2>
        <nav aria-label="Preferences settings">
          <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800">
            <li>
              <Link
                href="/settings/preferences"
                className="flex items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800"
              >
                <div>
                  <span>Manage Preferences</span>
                  {preferenceSummary && (
                    <p className="mt-0.5 text-xs font-normal text-gray-500 dark:text-gray-400">{preferenceSummary}</p>
                  )}
                </div>
                <ChevronRight />
              </Link>
            </li>
          </ul>
        </nav>
      </section>

      {/* ── Ingredients ─────────────────────────────────────────────────────── */}
      <section aria-labelledby="ingredients-heading" className="mt-6">
        <h2 id="ingredients-heading" className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Ingredients
        </h2>
        <nav aria-label="Ingredients settings">
          <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800">
            <li>
              <button
                type="button"
                onClick={() => setActivePanel('ingredient-catalogue')}
                className="flex w-full items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
              >
                <span>Ingredient Catalogue</span>
                <ChevronRight />
              </button>
            </li>
            <li>
              <button
                type="button"
                onClick={() => setActivePanel('ingredient-submit')}
                className="flex w-full items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
              >
                <span>Submit New Ingredient</span>
                <ChevronRight />
              </button>
            </li>
            <li>
              <button
                type="button"
                onClick={() => setActivePanel('my-submissions')}
                className="flex w-full items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
              >
                <span>My Submissions</span>
                <ChevronRight />
              </button>
            </li>
          </ul>
        </nav>
      </section>

      {/* ── Units ─────────────────────────────────────────────────────────────── */}
      <section aria-labelledby="units-heading" className="mt-6">
        <h2 id="units-heading" className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Units
        </h2>
        <div className="rounded-lg border border-gray-200 px-5 py-4 dark:border-gray-800">
          <div className="flex items-center justify-between gap-4">
            <span className="text-sm font-medium text-gray-900 dark:text-white">Measurement System</span>
            <UnitToggle />
          </div>
        </div>
      </section>

      {/* ── App ───────────────────────────────────────────────────────────────── */}
      <section aria-labelledby="app-heading" className="mt-6">
        <h2 id="app-heading" className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          App
        </h2>
        <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800">
          <li>
            <button
              type="button"
              onClick={handleReplaySplash}
              className="flex w-full items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
            >
              <span>Replay Introduction</span>
              <ChevronRight />
            </button>
          </li>
          <li>
            <button
              type="button"
              onClick={handleShare}
              className="flex w-full items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
            >
              <span>Share Blend</span>
              <ChevronRight />
            </button>
          </li>
        </ul>
      </section>

      {/* ── Danger Zone ───────────────────────────────────────────────────────── */}
      <section aria-labelledby="danger-heading" className="mt-8">
        <h2 id="danger-heading" className="mb-2 text-xs font-semibold uppercase tracking-wider text-red-600 dark:text-red-400">
          Danger Zone
        </h2>
        <div className={clsx(
          'rounded-lg border border-red-300 bg-red-50 px-5 py-4',
          'dark:border-red-800 dark:bg-red-900/20'
        )}>
          <p className="mb-3 text-sm text-red-700 dark:text-red-400">
            Permanently delete your account and all associated data. This action cannot be undone after the 30-day grace period.
          </p>
          <button
            type="button"
            onClick={() => setShowDeletionWizard(true)}
            className={clsx(
              'rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white',
              'hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2'
            )}
          >
            Delete my account
          </button>
        </div>
      </section>
    </div>
  )
}

function ChevronRight() {
  return (
    <svg
      className="h-4 w-4 flex-shrink-0 text-gray-400"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      aria-hidden="true"
    >
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
    </svg>
  )
}
