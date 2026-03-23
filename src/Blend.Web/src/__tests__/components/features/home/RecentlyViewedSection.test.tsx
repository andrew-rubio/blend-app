import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { RecentlyViewedSection } from '@/components/features/home/RecentlyViewedSection'
import type { HomeRecentlyViewedRecipe } from '@/types'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

const mockRecipes: HomeRecentlyViewedRecipe[] = [
  { recipeId: 'r1', referenceType: 'Recipe', viewedAt: '2024-01-01T00:00:00Z' },
  { recipeId: 'r2', referenceType: 'Recipe', viewedAt: '2024-01-02T00:00:00Z' },
]

describe('RecentlyViewedSection', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns null when recipes list is empty', () => {
    const { container } = render(<RecentlyViewedSection recipes={[]} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders section heading when there are recipes', () => {
    render(<RecentlyViewedSection recipes={mockRecipes} />)
    expect(screen.getByText('Recently Viewed')).toBeDefined()
  })

  it('renders correct number of recipe items', () => {
    render(<RecentlyViewedSection recipes={mockRecipes} />)
    const items = screen.getAllByRole('listitem')
    expect(items.length).toBe(2)
  })

  it('navigates to recipe on click', () => {
    render(<RecentlyViewedSection recipes={mockRecipes} />)
    const btn = screen.getByLabelText('View recipe r1')
    fireEvent.click(btn)
    expect(mockPush).toHaveBeenCalledWith('/recipes/r1')
  })

  it('has correct list role', () => {
    render(<RecentlyViewedSection recipes={mockRecipes} />)
    expect(screen.getByRole('list', { name: 'Recently viewed recipes' })).toBeDefined()
  })
})
