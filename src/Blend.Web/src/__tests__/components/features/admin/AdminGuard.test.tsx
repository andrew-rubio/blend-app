import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { AdminGuard } from '@/components/features/admin/AdminGuard'

const mockReplace = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ replace: mockReplace }),
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

import { useAuthStore } from '@/stores/authStore'
const mockUseAuthStore = vi.mocked(useAuthStore)

describe('AdminGuard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows loading spinner when auth is loading', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: false,
      user: null,
      isLoading: true,
    } as ReturnType<typeof useAuthStore>)

    const { container } = render(
      <AdminGuard>
        <div>Admin Content</div>
      </AdminGuard>
    )

    const spinner = container.querySelector('.animate-spin')
    expect(spinner).not.toBeNull()
    expect(screen.queryByText('Admin Content')).toBeNull()
  })

  it('renders children for admin users', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: { id: '1', name: 'Admin', email: 'admin@example.com', role: 'admin', createdAt: '' },
      isLoading: false,
    } as ReturnType<typeof useAuthStore>)

    render(
      <AdminGuard>
        <div>Admin Content</div>
      </AdminGuard>
    )

    expect(screen.getByText('Admin Content')).toBeDefined()
  })

  it('renders nothing for non-admin users', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: { id: '2', name: 'User', email: 'user@example.com', role: 'user', createdAt: '' },
      isLoading: false,
    } as ReturnType<typeof useAuthStore>)

    const { container } = render(
      <AdminGuard>
        <div>Admin Content</div>
      </AdminGuard>
    )

    expect(container.firstChild).toBeNull()
    expect(screen.queryByText('Admin Content')).toBeNull()
  })

  it('redirects unauthenticated users to /login', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: false,
      user: null,
      isLoading: false,
    } as ReturnType<typeof useAuthStore>)

    render(
      <AdminGuard>
        <div>Admin Content</div>
      </AdminGuard>
    )

    expect(mockReplace).toHaveBeenCalledWith('/login')
  })

  it('redirects non-admin authenticated users to /home', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: { id: '2', name: 'User', email: 'user@example.com', role: 'user', createdAt: '' },
      isLoading: false,
    } as ReturnType<typeof useAuthStore>)

    render(
      <AdminGuard>
        <div>Admin Content</div>
      </AdminGuard>
    )

    expect(mockReplace).toHaveBeenCalledWith('/home')
  })
})
