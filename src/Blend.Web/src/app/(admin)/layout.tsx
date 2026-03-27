'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { clsx } from 'clsx'
import type { ReactNode } from 'react'
import { AdminGuard } from '@/components/features/admin/AdminGuard'

const NAV_ITEMS = [
  { href: '/admin/dashboard', label: 'Dashboard' },
  { href: '/admin/featured-recipes', label: 'Featured Recipes' },
  { href: '/admin/stories', label: 'Stories' },
  { href: '/admin/videos', label: 'Videos' },
  { href: '/admin/ingredients', label: 'Ingredients' },
]

interface AdminLayoutProps {
  children: ReactNode
}

function AdminSidebar() {
  const pathname = usePathname()

  return (
    <aside className="w-64 shrink-0 border-r border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-950">
      <div className="p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Admin
        </h2>
      </div>
      <nav aria-label="Admin navigation">
        <ul className="space-y-1 px-2 pb-4">
          {NAV_ITEMS.map((item) => {
            const isActive = pathname === item.href
            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className={clsx(
                    'block rounded-md px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-primary-100 text-primary-700 dark:bg-primary-900 dark:text-primary-300'
                      : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100'
                  )}
                  aria-current={isActive ? 'page' : undefined}
                >
                  {item.label}
                </Link>
              </li>
            )
          })}
        </ul>
      </nav>
    </aside>
  )
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  return (
    <AdminGuard>
      <div className="flex min-h-screen">
        <AdminSidebar />
        <main className="flex-1 overflow-auto p-8">{children}</main>
      </div>
    </AdminGuard>
  )
}
