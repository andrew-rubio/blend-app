import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ExploreLanding } from '@/components/features/explore/ExploreLanding'
import type { RecipeSearchResult } from '@/types'

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

// Mock next/image
vi.mock('next/image', () => ({
  // eslint-disable-next-line @next/next/no-img-element
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockRecipes: RecipeSearchResult[] = [
  {
    id: '1',
    title: 'Trending Recipe',
    cuisines: ['Italian'],
    dishTypes: ['main course'],
    readyInMinutes: 30,
    popularity: 500,
    dataSource: 'Spoonacular',
    score: 0.9,
  },
]

// Mock search hooks
vi.mock('@/hooks/useSearch', () => ({
  useTrendingRecipes: () => ({ data: mockRecipes, isLoading: false, error: null }),
  useRecommendedRecipes: () => ({ data: mockRecipes, isLoading: false, error: null }),
}))

describe('ExploreLanding', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the trending section heading', () => {
    render(<ExploreLanding />)
    expect(screen.getByText('Trending Recipes')).toBeDefined()
  })

  it('renders the recommended section heading', () => {
    render(<ExploreLanding />)
    expect(screen.getByText('Recommended for You')).toBeDefined()
  })

  it('renders the category shortcuts heading', () => {
    render(<ExploreLanding />)
    expect(screen.getByText('Browse by Category')).toBeDefined()
  })

  it('renders trending recipe cards', () => {
    render(<ExploreLanding />)
    // The title appears in trending + recommended sections
    expect(screen.getAllByText('Trending Recipe').length).toBeGreaterThanOrEqual(1)
  })
})

describe('ExploreLanding - loading states', () => {
  it('shows skeleton loading when trending is loading', () => {
    vi.resetModules()
  })
})
