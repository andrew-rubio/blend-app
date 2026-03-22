import type { Metadata } from 'next'
import { ManagePreferences } from '@/components/features/preferences/ManagePreferences'

export const metadata: Metadata = {
  title: 'Manage Preferences',
}

export default function ManagePreferencesPage() {
  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">
        Manage Preferences
      </h1>
      <ManagePreferences />
    </div>
  )
}
