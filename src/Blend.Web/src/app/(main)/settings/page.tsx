import type { Metadata } from 'next'
import Link from 'next/link'

export const metadata: Metadata = {
  title: 'Settings',
}

export default function SettingsPage() {
  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">App Settings</h1>
      <nav aria-label="Settings navigation">
        <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 dark:divide-gray-800 dark:border-gray-800">
          <li>
            <Link
              href="/settings/preferences"
              className="flex items-center justify-between px-5 py-4 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-50 dark:text-white dark:hover:bg-gray-800"
            >
              <span>Manage Preferences</span>
              <svg
                className="h-4 w-4 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </Link>
          </li>
        </ul>
      </nav>
    </div>
  )
}
