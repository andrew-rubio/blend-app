'use client'

import { useState, useRef, useEffect } from 'react'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { clsx } from 'clsx'
import { useAuthStore } from '@/stores/authStore'
import { Button } from '@/components/ui/Button'
import { logoutApi } from '@/lib/api/auth'
import { NotificationBell } from '@/components/features/notifications/NotificationBell'

const NAV_ITEMS = [
  { href: '/home', label: 'Home' },
  { href: '/explore', label: 'Explore' },
  { href: '/cook', label: 'Cook' },
  { href: '/friends', label: 'Friends' },
  { href: '/profile', label: 'Profile' },
]

function AvatarDropdown({
  userName,
  onLogout,
}: {
  userName: string
  onLogout: () => void
}) {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const initials = userName
    .split(' ')
    .map((n) => n[0] ?? '')
    .filter(Boolean)
    .join('')
    .toUpperCase()
    .slice(0, 2) || '?'

  return (
    <div className="relative" ref={ref}>
      <button
        type="button"
        aria-haspopup="true"
        aria-expanded={open}
        aria-label="User menu"
        className="flex h-9 w-9 items-center justify-center rounded-full bg-primary-100 text-sm font-semibold text-primary-700 transition-colors hover:bg-primary-200 dark:bg-primary-900 dark:text-primary-300 dark:hover:bg-primary-800"
        onClick={() => setOpen((prev) => !prev)}
        data-testid="avatar-button"
      >
        {initials}
      </button>

      {open && (
        <div
          role="menu"
          aria-label="User menu"
          className="absolute right-0 mt-2 w-48 rounded-lg border border-gray-200 bg-white py-1 shadow-lg dark:border-gray-800 dark:bg-gray-950"
          data-testid="avatar-dropdown"
        >
          <div className="border-b border-gray-100 px-4 py-2 dark:border-gray-800">
            <p className="truncate text-sm font-medium text-gray-900 dark:text-white">{userName}</p>
          </div>
          <Link
            href="/profile"
            role="menuitem"
            className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-gray-900"
            onClick={() => setOpen(false)}
          >
            View Profile
          </Link>
          <Link
            href="/settings"
            role="menuitem"
            className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-gray-900"
            onClick={() => setOpen(false)}
          >
            Settings
          </Link>
          <button
            type="button"
            role="menuitem"
            className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-gray-50 dark:text-red-400 dark:hover:bg-gray-900"
            onClick={() => {
              setOpen(false)
              onLogout()
            }}
          >
            Logout
          </button>
        </div>
      )}
    </div>
  )
}

export function Header() {
  const pathname = usePathname()
  const router = useRouter()
  const { isAuthenticated, user, logout } = useAuthStore()

  const handleLogout = async () => {
    try {
      await logoutApi()
    } catch {
      // proceed with client-side logout even if the API call fails
    }
    logout()
    router.push('/login')
  }

  const isActive = (href: string) => pathname.startsWith(href)

  return (
    <header className="sticky top-0 z-50 border-b border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-950">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link href="/home" className="flex items-center gap-2" aria-label="Blend home">
          <span className="text-2xl font-bold text-primary-600">Blend</span>
        </Link>

        <nav aria-label="Main navigation" className="hidden items-center gap-1 md:flex">
          {NAV_ITEMS.map((item) => {
            const active = isActive(item.href)
            const isCook = item.href === '/cook'
            return (
              <Link
                key={item.href}
                href={item.href}
                aria-current={active ? 'page' : undefined}
                className={clsx(
                  'rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isCook
                    ? active
                      ? 'bg-primary-700 text-white dark:bg-primary-400 dark:text-white'
                      : 'bg-primary-600 text-white hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-600'
                    : active
                      ? 'bg-primary-50 text-primary-700 dark:bg-primary-950 dark:text-primary-300'
                      : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100'
                )}
              >
                {item.label}
              </Link>
            )
          })}
          {user?.role === 'admin' && (
            <Link
              href="/admin/dashboard"
              aria-current={isActive('/admin') ? 'page' : undefined}
              className={clsx(
                'rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive('/admin')
                  ? 'bg-primary-50 text-primary-700 dark:bg-primary-950 dark:text-primary-300'
                  : 'text-primary-600 hover:bg-primary-50 hover:text-primary-700 dark:text-primary-400 dark:hover:bg-primary-950 dark:hover:text-primary-300'
              )}
            >
              Admin
            </Link>
          )}
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated && user ? (
            <>
              <NotificationBell />
              <AvatarDropdown userName={user.name} onLogout={handleLogout} />
            </>
          ) : (
            <>
              <Link href="/login">
                <Button variant="outline" size="sm">
                  Sign in
                </Button>
              </Link>
              <Link href="/register">
                <Button variant="primary" size="sm">
                  Get started
                </Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
