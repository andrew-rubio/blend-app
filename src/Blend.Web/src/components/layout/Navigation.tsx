'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { clsx } from 'clsx'

interface NavItem {
  href: string
  label: string
}

const navItems: NavItem[] = [
  { href: '/home', label: 'Home' },
  { href: '/explore', label: 'Explore' },
]

export function Navigation() {
  const pathname = usePathname()

  return (
    <nav className="flex items-center gap-1">
      {navItems.map((item) => (
        <Link
          key={item.href}
          href={item.href}
          className={clsx(
            'rounded-md px-3 py-2 text-sm font-medium transition-colors',
            pathname === item.href
              ? 'bg-primary-50 text-primary-700 dark:bg-primary-950 dark:text-primary-300'
              : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100'
          )}
        >
          {item.label}
        </Link>
      ))}
    </nav>
  )
}
