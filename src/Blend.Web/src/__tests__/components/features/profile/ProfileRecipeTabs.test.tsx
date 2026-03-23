import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ProfileRecipeTabs } from '@/components/features/profile/ProfileRecipeTabs'
import type { ProfileRecipesResponse } from '@/types'

vi.mock('next/image', () => ({
  default: ({ alt }: { alt: string }) => <img alt={alt} />,
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

vi.mock('@/hooks/useProfile', () => ({
  useMyRecipes: vi.fn(),
  useLikedRecipes: vi.fn(),
  useToggleRecipeVisibility: vi.fn(),
  useDeleteRecipe: vi.fn(),
}))

import { useMyRecipes, useLikedRecipes, useToggleRecipeVisibility, useDeleteRecipe } from '@/hooks/useProfile'

const mockUseMyRecipes = vi.mocked(useMyRecipes)
const mockUseLikedRecipes = vi.mocked(useLikedRecipes)
const mockUseToggleVisibility = vi.mocked(useToggleRecipeVisibility)
const mockUseDeleteRecipe = vi.mocked(useDeleteRecipe)

const sampleRecipes = [
  { id: 'r1', title: 'Pasta', imageUrl: undefined, cuisines: ['Italian'], likeCount: 5, isPublic: true, createdAt: '2024-01-01' },
  { id: 'r2', title: 'Pizza', imageUrl: undefined, cuisines: ['Italian'], likeCount: 10, isPublic: false, createdAt: '2024-01-02' },
]

const emptyPage: ProfileRecipesResponse = { recipes: [], hasMore: false }
const filledPage: ProfileRecipesResponse = { recipes: sampleRecipes, hasMore: false }

function makeInfiniteQueryResult(pages: ProfileRecipesResponse[]) {
  return {
    data: { pages, pageParams: [undefined] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
    isFetchingNextPage: false,
  }
}

describe('ProfileRecipeTabs', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseMyRecipes.mockReturnValue(makeInfiniteQueryResult([filledPage]) as unknown as ReturnType<typeof useMyRecipes>)
    mockUseLikedRecipes.mockReturnValue(makeInfiniteQueryResult([emptyPage]) as unknown as ReturnType<typeof useLikedRecipes>)
    mockUseToggleVisibility.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useToggleRecipeVisibility>)
    mockUseDeleteRecipe.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useDeleteRecipe>)
  })

  it('renders My Recipes and Liked Recipes tabs', () => {
    render(<ProfileRecipeTabs />)
    expect(screen.getByRole('tab', { name: 'My Recipes' })).toBeDefined()
    expect(screen.getByRole('tab', { name: 'Liked Recipes' })).toBeDefined()
  })

  it('My Recipes tab is active by default', () => {
    render(<ProfileRecipeTabs />)
    expect(screen.getByRole('tab', { name: 'My Recipes' })).toHaveAttribute('aria-selected', 'true')
  })

  it('renders recipe titles in My Recipes tab', () => {
    render(<ProfileRecipeTabs />)
    expect(screen.getByText('Pasta')).toBeDefined()
    expect(screen.getByText('Pizza')).toBeDefined()
  })

  it('switches to Liked Recipes tab on click', () => {
    render(<ProfileRecipeTabs />)
    fireEvent.click(screen.getByRole('tab', { name: 'Liked Recipes' }))
    expect(screen.getByRole('tab', { name: 'Liked Recipes' })).toHaveAttribute('aria-selected', 'true')
  })

  it('shows empty state for liked recipes when empty', () => {
    render(<ProfileRecipeTabs />)
    fireEvent.click(screen.getByRole('tab', { name: 'Liked Recipes' }))
    expect(screen.getByText('No liked recipes')).toBeDefined()
  })

  it('shows empty state for my recipes when empty', () => {
    mockUseMyRecipes.mockReturnValue(makeInfiniteQueryResult([emptyPage]) as unknown as ReturnType<typeof useMyRecipes>)
    render(<ProfileRecipeTabs />)
    expect(screen.getByText('No recipes yet')).toBeDefined()
  })

  it('shows loading state', () => {
    mockUseMyRecipes.mockReturnValue({
      data: undefined,
      isLoading: true,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof useMyRecipes>)
    render(<ProfileRecipeTabs />)
    // Loading skeletons should be present
    const skeletons = document.querySelectorAll('[aria-hidden="true"]')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('shows context menu on my recipe cards', () => {
    render(<ProfileRecipeTabs />)
    const menuButtons = screen.getAllByLabelText('Recipe options')
    expect(menuButtons.length).toBe(2)
  })

  it('shows delete confirmation dialog when delete is clicked', () => {
    render(<ProfileRecipeTabs />)
    // Open context menu for first recipe
    fireEvent.click(screen.getAllByLabelText('Recipe options')[0])
    // Click delete
    fireEvent.click(screen.getByLabelText('Delete recipe'))
    // Dialog should appear
    expect(screen.getByLabelText('Confirm recipe deletion')).toBeDefined()
    expect(screen.getByText(/30 days to contact support/)).toBeDefined()
  })

  it('closes delete dialog on cancel', () => {
    render(<ProfileRecipeTabs />)
    fireEvent.click(screen.getAllByLabelText('Recipe options')[0])
    fireEvent.click(screen.getByLabelText('Delete recipe'))
    fireEvent.click(screen.getByLabelText('Cancel deletion'))
    expect(screen.queryByLabelText('Confirm recipe deletion')).toBeNull()
  })

  it('calls deleteRecipe on confirm deletion', () => {
    const mockDelete = vi.fn()
    mockUseDeleteRecipe.mockReturnValue({ mutate: mockDelete, isPending: false } as unknown as ReturnType<typeof useDeleteRecipe>)
    render(<ProfileRecipeTabs />)
    fireEvent.click(screen.getAllByLabelText('Recipe options')[0])
    fireEvent.click(screen.getByLabelText('Delete recipe'))
    fireEvent.click(screen.getByLabelText('Confirm delete recipe'))
    expect(mockDelete).toHaveBeenCalledWith('r1', expect.any(Object))
  })
})
