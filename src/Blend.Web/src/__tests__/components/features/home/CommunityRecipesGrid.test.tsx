import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { CommunityRecipesGrid, CommunityRecipesGridSkeleton } from '@/components/features/home/CommunityRecipesGrid'
import type { HomeCommunityRecipe } from '@/types'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))
vi.mock('next/link', () => ({
  default: ({ children, href, className, 'aria-label': ariaLabel }: { children: React.ReactNode; href: string; className?: string; 'aria-label'?: string }) => (
    <a href={href} className={className} aria-label={ariaLabel}>{children}</a>
  ),
}))
vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockRecipes: HomeCommunityRecipe[] = [
  { id: 'r1', title: 'Vegan Bowl', cuisineType: 'American', likeCount: 42, imageUrl: 'https://example.com/bowl.jpg' },
  { id: 'r2', title: 'Sushi Rolls', cuisineType: 'Japanese', likeCount: 88 },
]

describe('CommunityRecipesGrid', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns null when recipes list is empty', () => {
    const { container } = render(<CommunityRecipesGrid recipes={[]} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders section heading', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    expect(screen.getByText('Community Recipes')).toBeDefined()
  })

  it('renders "See all" link', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    const seeAll = screen.getByRole('link', { name: 'See all community recipes' })
    expect(seeAll).toBeDefined()
    expect(seeAll.getAttribute('href')).toBe('/explore?source=community')
  })

  it('renders recipe titles', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    expect(screen.getByText('Vegan Bowl')).toBeDefined()
    expect(screen.getByText('Sushi Rolls')).toBeDefined()
  })

  it('renders cuisine type tags', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    expect(screen.getByText('American')).toBeDefined()
  })

  it('renders like counts', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    expect(screen.getByLabelText('42 likes')).toBeDefined()
  })

  it('navigates to recipe on click', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    const article = screen.getByRole('article', { name: 'Vegan Bowl' })
    fireEvent.click(article)
    expect(mockPush).toHaveBeenCalledWith('/recipes/r1')
  })

  it('navigates via keyboard Enter', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    const article = screen.getByRole('article', { name: 'Vegan Bowl' })
    fireEvent.keyDown(article, { key: 'Enter' })
    expect(mockPush).toHaveBeenCalledWith('/recipes/r1')
  })

  it('navigates via keyboard Space', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    const article = screen.getByRole('article', { name: 'Vegan Bowl' })
    fireEvent.keyDown(article, { key: ' ' })
    expect(mockPush).toHaveBeenCalledWith('/recipes/r1')
  })

  it('has correct grid list role', () => {
    render(<CommunityRecipesGrid recipes={mockRecipes} />)
    expect(screen.getByRole('list', { name: 'Community recipes' })).toBeDefined()
  })
})

describe('CommunityRecipesGridSkeleton', () => {
  it('renders skeleton UI', () => {
    const { container } = render(<CommunityRecipesGridSkeleton />)
    expect(container.firstChild).toBeDefined()
  })
})
