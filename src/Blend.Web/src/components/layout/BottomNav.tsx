'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { clsx } from 'clsx'
import { useAuthStore } from '@/stores/authStore'
import { GuestPromptModal, useGuestPrompt } from '@/components/features/GuestPromptModal'

interface BottomNavItem {
  href: string
  label: string
  requiresAuth: boolean
  icon: React.ReactNode
}

function HomeIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden="true"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
      />
    </svg>
  )
}

function ExploreIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden="true"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
  )
}

function FlameIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden="true"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M17.657 18.657A8 8 0 016.343 7.343S7 9 9 10c0-2 .5-5 2.986-7C14 5 16.09 5.777 17.656 7.343A7.975 7.975 0 0120 13a7.975 7.975 0 01-2.343 5.657z"
      />
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M9.879 16.121A3 3 0 1012.015 11L11 14H9c0 .768.293 1.536.879 2.121z"
      />
    </svg>
  )
}

function FriendsIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden="true"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
      />
    </svg>
  )
}

function ProfileIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden="true"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
      />
    </svg>
  )
}

const NAV_ITEMS: BottomNavItem[] = [
  { href: '/home', label: 'Home', requiresAuth: false, icon: <HomeIcon /> },
  { href: '/explore', label: 'Explore', requiresAuth: false, icon: <ExploreIcon /> },
  { href: '/cook', label: 'Cook', requiresAuth: true, icon: <FlameIcon /> },
  { href: '/friends', label: 'Friends', requiresAuth: true, icon: <FriendsIcon /> },
  { href: '/profile', label: 'Profile', requiresAuth: true, icon: <ProfileIcon /> },
]

const RESTRICTED_MESSAGES: Record<string, string> = {
  '/cook': 'Sign in to access Cook Mode and start tracking your cooking sessions.',
  '/friends': 'Sign in to connect with friends and share your culinary journey.',
  '/profile': 'Sign in to view and manage your profile.',
}

export function BottomNav() {
  const pathname = usePathname()
  const { isAuthenticated } = useAuthStore()
  const { isOpen, message, prompt, close } = useGuestPrompt()

  const isActive = (href: string) => pathname.startsWith(href)

  return (
    <>
      <nav
        aria-label="Main navigation"
        className="fixed bottom-0 left-0 right-0 z-50 border-t border-gray-200 bg-white pb-safe dark:border-gray-800 dark:bg-gray-950 md:hidden"
        data-testid="bottom-nav"
      >
        <ul className="flex h-16 items-center justify-around">
          {NAV_ITEMS.map((item) => {
            const active = isActive(item.href)
            const isCook = item.href === '/cook'

            if (item.requiresAuth && !isAuthenticated) {
              return (
                <li key={item.href}>
                  <button
                    type="button"
                    aria-label={item.label}
                    className={clsx(
                      'flex flex-col items-center justify-center gap-0.5 transition-colors',
                      isCook
                        ? 'relative -top-3 h-14 w-14 rounded-full shadow-lg'
                        : 'min-w-[4rem] py-1',
                      isCook
                        ? 'bg-primary-600 text-white hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-600'
                        : 'text-gray-500 hover:text-primary-600 dark:text-gray-400 dark:hover:text-primary-400'
                    )}
                    onClick={() => prompt(RESTRICTED_MESSAGES[item.href])}
                  >
                    {item.icon}
                    {!isCook && (
                      <span className="text-[10px] font-medium leading-tight">{item.label}</span>
                    )}
                  </button>
                </li>
              )
            }

            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  aria-label={item.label}
                  aria-current={active ? 'page' : undefined}
                  className={clsx(
                    'flex flex-col items-center justify-center gap-0.5 transition-colors',
                    isCook
                      ? 'relative -top-3 h-14 w-14 rounded-full shadow-lg'
                      : 'min-w-[4rem] py-1',
                    isCook
                      ? active
                        ? 'bg-primary-700 text-white dark:bg-primary-400'
                        : 'bg-primary-600 text-white hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-600'
                      : active
                        ? 'text-primary-600 dark:text-primary-400'
                        : 'text-gray-500 hover:text-primary-600 dark:text-gray-400 dark:hover:text-primary-400'
                  )}
                >
                  {item.icon}
                  {!isCook && (
                    <span className="text-[10px] font-medium leading-tight">{item.label}</span>
                  )}
                </Link>
              </li>
            )
          })}
        </ul>
      </nav>

      <GuestPromptModal isOpen={isOpen} onClose={close} message={message} />
    </>
  )
}
