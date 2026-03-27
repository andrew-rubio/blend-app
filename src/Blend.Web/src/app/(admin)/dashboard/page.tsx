'use client'

import Link from 'next/link'
import { useAdminDashboardCounts } from '@/hooks/useAdmin'
import { Card } from '@/components/ui/Card'

interface CountCardProps {
  label: string
  count: number | undefined
  href: string
  isLoading: boolean
}

function CountCard({ label, count, href, isLoading }: CountCardProps) {
  return (
    <Link href={href} className="block">
      <Card className="transition-shadow hover:shadow-md">
        <p className="text-sm font-medium text-gray-500 dark:text-gray-400">{label}</p>
        {isLoading ? (
          <div className="mt-2 h-8 w-16 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        ) : (
          <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">{count ?? 0}</p>
        )}
      </Card>
    </Link>
  )
}

export default function DashboardPage() {
  const { data: counts, isLoading } = useAdminDashboardCounts()

  return (
    <div>
      <h1 className="mb-6 text-3xl font-bold text-gray-900 dark:text-white">Admin Dashboard</h1>
      <p className="mb-8 text-gray-600 dark:text-gray-400">
        Manage featured content and ingredient submissions.
      </p>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <CountCard
          label="Featured Recipes"
          count={counts?.featuredRecipes}
          href="/admin/featured-recipes"
          isLoading={isLoading}
        />
        <CountCard
          label="Stories"
          count={counts?.stories}
          href="/admin/stories"
          isLoading={isLoading}
        />
        <CountCard
          label="Videos"
          count={counts?.videos}
          href="/admin/videos"
          isLoading={isLoading}
        />
        <CountCard
          label="Pending Submissions"
          count={counts?.pendingSubmissions}
          href="/admin/ingredients"
          isLoading={isLoading}
        />
      </div>

      <div className="mt-10">
        <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">
          Quick Links
        </h2>
        <ul className="space-y-2">
          {[
            { href: '/admin/featured-recipes', label: 'Manage Featured Recipes' },
            { href: '/admin/stories', label: 'Manage Stories' },
            { href: '/admin/videos', label: 'Manage Videos' },
            { href: '/admin/ingredients', label: 'Review Ingredient Submissions' },
          ].map((link) => (
            <li key={link.href}>
              <Link
                href={link.href}
                className="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300"
              >
                {link.label} →
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
