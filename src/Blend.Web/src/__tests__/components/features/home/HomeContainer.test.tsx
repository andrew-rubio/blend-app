import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { HomeContainer } from '@/components/features/home/HomeContainer'
import type { HomeResponse } from '@/types'

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))
vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))
vi.mock('next/link', () => ({
  default: ({ children, href, className, 'aria-label': ariaLabel }: { children: React.ReactNode; href: string; className?: string; 'aria-label'?: string }) => (
    <a href={href} className={className} aria-label={ariaLabel}>{children}</a>
  ),
}))
vi.mock('@/hooks/useHome', () => ({
  useHome: vi.fn(),
  useRefreshHome: () => vi.fn().mockResolvedValue(undefined),
}))

import { useHome } from '@/hooks/useHome'
const mockUseHome = vi.mocked(useHome)

const mockHomeData: HomeResponse = {
  search: { placeholder: 'Find your next meal...' },
  featured: {
    recipes: [
      { id: 'fr1', title: 'Featured Pasta', attribution: 'Chef Mario' },
    ],
    stories: [
      { id: 'fs1', title: 'Story of Ramen', author: 'Jane', readingTimeMinutes: 3 },
    ],
    videos: [
      { id: 'fv1', title: 'How to Cook Steak', creator: 'Gordon' },
    ],
  },
  community: {
    recipes: [
      { id: 'cr1', title: 'Community Bowl', likeCount: 10 },
    ],
  },
  recentlyViewed: {
    recipes: [
      { recipeId: 'rv1', referenceType: 'Recipe', viewedAt: '2024-01-01T00:00:00Z' },
    ],
  },
}

describe('HomeContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows skeleton loading state', () => {
    mockUseHome.mockReturnValue({ data: undefined, isLoading: true, error: null } as ReturnType<typeof useHome>)
    const { container } = render(<HomeContainer />)
    // Skeletons use animate-pulse
    const skeletons = container.querySelectorAll('.animate-pulse')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders all sections when data loads', () => {
    mockUseHome.mockReturnValue({ data: mockHomeData, isLoading: false, error: null } as ReturnType<typeof useHome>)
    render(<HomeContainer />)

    // Search bar
    expect(screen.getByRole('search')).toBeDefined()

    // Featured carousel
    expect(screen.getByText('Featured Pasta')).toBeDefined()

    // Featured stories
    expect(screen.getByText('Featured Stories')).toBeDefined()
    expect(screen.getByText('Story of Ramen')).toBeDefined()

    // Featured videos
    expect(screen.getByText('Featured Videos')).toBeDefined()
    expect(screen.getByText('How to Cook Steak')).toBeDefined()

    // Community recipes
    expect(screen.getByText('Community Recipes')).toBeDefined()
    expect(screen.getByText('Community Bowl')).toBeDefined()

    // Recently viewed
    expect(screen.getByText('Recently Viewed')).toBeDefined()
  })

  it('shows error message on failure', () => {
    mockUseHome.mockReturnValue({ data: undefined, isLoading: false, error: { message: 'Network error', status: 500 } } as ReturnType<typeof useHome>)
    render(<HomeContainer />)
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Could not load home content. Please try again.')).toBeDefined()
  })

  it('hides recently viewed section when empty', () => {
    const dataWithEmptyRecentlyViewed = {
      ...mockHomeData,
      recentlyViewed: { recipes: [] },
    }
    mockUseHome.mockReturnValue({ data: dataWithEmptyRecentlyViewed, isLoading: false, error: null } as ReturnType<typeof useHome>)
    render(<HomeContainer />)
    expect(screen.queryByText('Recently Viewed')).toBeNull()
  })

  it('hides sections with empty data gracefully', () => {
    const emptyData: HomeResponse = {
      search: { placeholder: 'Search...' },
      featured: { recipes: [], stories: [], videos: [] },
      community: { recipes: [] },
      recentlyViewed: { recipes: [] },
    }
    mockUseHome.mockReturnValue({ data: emptyData, isLoading: false, error: null } as ReturnType<typeof useHome>)
    render(<HomeContainer />)
    // Only search bar should be visible
    expect(screen.getByRole('search')).toBeDefined()
    expect(screen.queryByText('Featured Stories')).toBeNull()
    expect(screen.queryByText('Community Recipes')).toBeNull()
    expect(screen.queryByText('Recently Viewed')).toBeNull()
  })
})
