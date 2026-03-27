'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import type { ReactNode } from 'react'
import { useAuthStore } from '@/stores/authStore'

interface AdminGuardProps {
  children: ReactNode
}

/**
 * Wraps admin pages to ensure only users with the 'admin' role can access them.
 * Non-admin authenticated users are redirected to /home.
 * Unauthenticated users are redirected to /login.
 */
export function AdminGuard({ children }: AdminGuardProps) {
  const router = useRouter()
  const { isAuthenticated, user, isLoading } = useAuthStore()

  useEffect(() => {
    if (isLoading) return
    if (!isAuthenticated) {
      router.replace('/login')
      return
    }
    if (user?.role !== 'admin') {
      router.replace('/home')
    }
  }, [isAuthenticated, user, isLoading, router])

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <span className="h-8 w-8 animate-spin rounded-full border-4 border-primary-600 border-t-transparent" />
      </div>
    )
  }

  if (!isAuthenticated || user?.role !== 'admin') {
    return null
  }

  return <>{children}</>
}
