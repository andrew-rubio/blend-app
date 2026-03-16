import type { ReactNode } from 'react'

interface AdminLayoutProps {
  children: ReactNode
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  return (
    <div className="flex min-h-screen">
      <aside className="w-64 border-r border-gray-200 bg-gray-50 dark:border-gray-800 dark:bg-gray-950">
        <div className="p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
            Admin
          </h2>
        </div>
      </aside>
      <main className="flex-1 p-8">{children}</main>
    </div>
  )
}
