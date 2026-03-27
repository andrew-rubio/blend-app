import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import DashboardPage from '@/app/(admin)/dashboard/page'
import type { AdminDashboardCounts } from '@/types'

vi.mock('next/link', () => ({
  default: ({
    children,
    href,
    className,
  }: {
    children: React.ReactNode
    href: string
    className?: string
  }) => (
    <a href={href} className={className}>
      {children}
    </a>
  ),
}))

vi.mock('@/hooks/useAdmin', () => ({
  useAdminDashboardCounts: vi.fn(),
}))

import { useAdminDashboardCounts } from '@/hooks/useAdmin'
const mockUseCounts = vi.mocked(useAdminDashboardCounts)

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders page heading', () => {
    mockUseCounts.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as ReturnType<typeof useAdminDashboardCounts>)

    render(<DashboardPage />)
    expect(screen.getByText('Admin Dashboard')).toBeDefined()
  })

  it('shows loading skeletons when data is loading', () => {
    mockUseCounts.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as ReturnType<typeof useAdminDashboardCounts>)

    const { container } = render(<DashboardPage />)
    const skeletons = container.querySelectorAll('.animate-pulse')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders content count cards when data loads', () => {
    const counts: AdminDashboardCounts = {
      featuredRecipes: 5,
      stories: 12,
      videos: 3,
      pendingSubmissions: 8,
    }
    mockUseCounts.mockReturnValue({
      data: counts,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useAdminDashboardCounts>)

    render(<DashboardPage />)
    expect(screen.getByText('5')).toBeDefined()
    expect(screen.getByText('12')).toBeDefined()
    expect(screen.getByText('3')).toBeDefined()
    expect(screen.getByText('8')).toBeDefined()
  })

  it('renders quick links to management sections', () => {
    mockUseCounts.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useAdminDashboardCounts>)

    render(<DashboardPage />)
    expect(screen.getByText('Manage Featured Recipes →')).toBeDefined()
    expect(screen.getByText('Manage Stories →')).toBeDefined()
    expect(screen.getByText('Manage Videos →')).toBeDefined()
    expect(screen.getByText('Review Ingredient Submissions →')).toBeDefined()
  })

  it('renders card labels', () => {
    mockUseCounts.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useAdminDashboardCounts>)

    render(<DashboardPage />)
    expect(screen.getByText('Featured Recipes')).toBeDefined()
    expect(screen.getByText('Stories')).toBeDefined()
    expect(screen.getByText('Videos')).toBeDefined()
    expect(screen.getByText('Pending Submissions')).toBeDefined()
  })
})
