import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { RecipeDetailContainer } from '@/components/features/recipe/RecipeDetailContainer'
import type { Recipe } from '@/types'

// Mocks
vi.mock('next/link', () => ({
  default: ({ children, href, onClick, className, 'aria-label': ariaLabel }: {
    children: React.ReactNode
    href: string
    onClick?: () => void
    className?: string
    'aria-label'?: string
  }) => (
    <a href={href} onClick={onClick} className={className} aria-label={ariaLabel}>
      {children}
    </a>
  ),
}))

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('@/hooks/useRecipe', () => ({
  useRecipe: vi.fn(),
  useLikeRecipe: () => ({ mutate: vi.fn(), isPending: false }),
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

import { useRecipe } from '@/hooks/useRecipe'
import { useAuthStore } from '@/stores/authStore'

const mockUseRecipe = vi.mocked(useRecipe)
const mockUseAuthStore = vi.mocked(useAuthStore)

const mockRecipe: Recipe = {
  id: '42',
  title: 'Spaghetti Carbonara',
  description: 'Classic Italian pasta.',
  imageUrl: 'https://example.com/carbonara.jpg',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  diets: ['gluten free'],
  intolerances: [],
  servings: 4,
  readyInMinutes: 30,
  difficulty: 'Medium',
  ingredients: [
    { id: 'i1', name: 'pasta', amount: 200, unit: 'g' },
  ],
  steps: [
    { number: 1, step: 'Boil water.' },
  ],
  dataSource: 'Spoonacular',
  likeCount: 50,
  isLiked: false,
}

describe('RecipeDetailContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: null,
      token: null,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    })
  })

  it('shows skeleton while loading', () => {
    mockUseRecipe.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('Loading recipe')).toBeDefined()
  })

  it('shows 404 error state', () => {
    mockUseRecipe.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 404, message: 'Not found' },
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="99" />)
    expect(screen.getByText('Recipe not found')).toBeDefined()
  })

  it('shows 403 error state', () => {
    mockUseRecipe.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 403, message: 'Forbidden' },
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="1" />)
    expect(screen.getByText('This recipe is private')).toBeDefined()
  })

  it('shows generic error state', () => {
    mockUseRecipe.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 500, message: 'Server error' },
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="1" />)
    expect(screen.getByText(/Something went wrong/)).toBeDefined()
  })

  it('renders recipe title when loaded', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByText('Spaghetti Carbonara')).toBeDefined()
  })

  it('renders cuisine tags', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByText('Italian')).toBeDefined()
  })

  it('renders Spoonacular source label for Spoonacular recipes', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('Source: Spoonacular')).toBeDefined()
  })

  it('renders community author link for community recipes', () => {
    const communityRecipe: Recipe = {
      ...mockRecipe,
      dataSource: 'Community',
      author: { id: 'u1', name: 'John Doe', avatarUrl: undefined },
    }
    mockUseRecipe.mockReturnValue({
      data: communityRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('View profile of John Doe')).toBeDefined()
  })

  it('renders all three tab buttons', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByRole('tab', { name: 'Overview' })).toBeDefined()
    expect(screen.getByRole('tab', { name: 'Ingredients' })).toBeDefined()
    expect(screen.getByRole('tab', { name: 'Directions' })).toBeDefined()
  })

  it('overview tab is active by default', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    const overviewTab = screen.getByRole('tab', { name: 'Overview' })
    expect(overviewTab).toHaveAttribute('aria-selected', 'true')
  })

  it('switches to ingredients tab on click', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    fireEvent.click(screen.getByRole('tab', { name: 'Ingredients' }))
    expect(screen.getByRole('tab', { name: 'Ingredients' })).toHaveAttribute('aria-selected', 'true')
    // Serving adjuster is a unique element in the Ingredients tab
    expect(screen.getByLabelText('Serving adjuster')).toBeDefined()
  })

  it('switches to directions tab on click', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    fireEvent.click(screen.getByRole('tab', { name: 'Directions' }))
    expect(screen.getByRole('tab', { name: 'Directions' })).toHaveAttribute('aria-selected', 'true')
    // Step text is a unique element in the Directions tab
    expect(screen.getByText('Boil water.')).toBeDefined()
  })

  it('renders like button with count', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('Like recipe')).toBeDefined()
    expect(screen.getByText(/50/)).toBeDefined()
  })

  it('shows unlike button label when recipe is liked', () => {
    mockUseRecipe.mockReturnValue({
      data: { ...mockRecipe, isLiked: true },
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('Unlike recipe')).toBeDefined()
  })

  it('renders Share button', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByLabelText('Share recipe')).toBeDefined()
  })

  it('renders Cook this dish button for authenticated users', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    expect(screen.getByText('Cook this dish')).toBeDefined()
  })

  it('shows guest prompt modal when unauthenticated user clicks like', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: false,
      user: null,
      token: null,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    })
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    fireEvent.click(screen.getByLabelText('Like recipe'))
    expect(screen.getByTestId('guest-prompt-modal')).toBeDefined()
  })

  it('shows guest prompt modal when unauthenticated user clicks Cook this dish', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: false,
      user: null,
      token: null,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    })
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    fireEvent.click(screen.getByLabelText('Sign in to cook this dish'))
    expect(screen.getByTestId('guest-prompt-modal')).toBeDefined()
  })

  it('navigates to cook mode when authenticated user clicks Cook this dish', () => {
    mockUseRecipe.mockReturnValue({
      data: mockRecipe,
      isLoading: false,
      error: null,
    } as ReturnType<typeof useRecipe>)
    render(<RecipeDetailContainer id="42" />)
    fireEvent.click(screen.getByText('Cook this dish'))
    expect(mockPush).toHaveBeenCalledWith('/cook/42')
  })
})
