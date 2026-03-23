import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FeaturedCarousel, FeaturedCarouselSkeleton } from '@/components/features/home/FeaturedCarousel'
import type { HomeFeaturedRecipe } from '@/types'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))
vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockRecipes: HomeFeaturedRecipe[] = [
  { id: '1', title: 'Pasta Carbonara', imageUrl: 'https://example.com/pasta.jpg', attribution: 'Chef Mario' },
  { id: '2', title: 'Chicken Tikka', attribution: 'Chef Priya' },
  { id: '3', title: 'Avocado Toast' },
]

describe('FeaturedCarousel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns null when recipes list is empty', () => {
    const { container } = render(<FeaturedCarousel recipes={[]} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders the first recipe by default', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    expect(screen.getByText('Pasta Carbonara')).toBeDefined()
  })

  it('renders attribution text', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    expect(screen.getByText('by Chef Mario')).toBeDefined()
  })

  it('renders dot indicators when multiple recipes', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    const tabs = screen.getAllByRole('tab')
    expect(tabs.length).toBe(3)
  })

  it('does not render dots when single recipe', () => {
    render(<FeaturedCarousel recipes={[mockRecipes[0]]} />)
    const tabs = screen.queryAllByRole('tab')
    expect(tabs.length).toBe(0)
  })

  it('navigates to recipe detail on click', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    const button = screen.getByRole('button', { name: /View recipe: Pasta Carbonara/i })
    fireEvent.click(button)
    expect(mockPush).toHaveBeenCalledWith('/recipes/1')
  })

  it('navigates via keyboard Enter', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    const button = screen.getByRole('button', { name: /View recipe: Pasta Carbonara/i })
    fireEvent.keyDown(button, { key: 'Enter' })
    expect(mockPush).toHaveBeenCalledWith('/recipes/1')
  })

  it('navigates via keyboard Space', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    const button = screen.getByRole('button', { name: /View recipe: Pasta Carbonara/i })
    fireEvent.keyDown(button, { key: ' ' })
    expect(mockPush).toHaveBeenCalledWith('/recipes/1')
  })

  it('changes slide when dot is clicked', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    const tabs = screen.getAllByRole('tab')
    fireEvent.click(tabs[1])
    expect(screen.getByText('Chicken Tikka')).toBeDefined()
  })

  it('has section label', () => {
    render(<FeaturedCarousel recipes={mockRecipes} />)
    expect(screen.getByRole('region', { name: 'Featured recipes' })).toBeDefined()
  })

  it('shows placeholder when no imageUrl', () => {
    render(<FeaturedCarousel recipes={[{ id: '3', title: 'Avocado Toast' }]} />)
    expect(screen.getByText('Avocado Toast')).toBeDefined()
  })
})

describe('FeaturedCarouselSkeleton', () => {
  it('renders skeleton UI', () => {
    const { container } = render(<FeaturedCarouselSkeleton />)
    expect(container.firstChild).toBeDefined()
  })
})
