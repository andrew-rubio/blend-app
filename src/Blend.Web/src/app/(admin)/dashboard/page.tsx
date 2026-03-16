import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Admin Dashboard',
}

export default function DashboardPage() {
  return (
    <div>
      <h1 className="mb-6 text-3xl font-bold text-gray-900 dark:text-white">Admin Dashboard</h1>
      <p className="text-gray-600 dark:text-gray-400">
        Manage users, recipes, and platform settings.
      </p>
    </div>
  )
}
