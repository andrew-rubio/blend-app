import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { RecipeDetailPage } from '../RecipeDetailPage'
import type { Recipe } from '@/types/recipe'

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

const mockRecipe: Recipe = {
  id: '1',
  title: 'Integration Test Recipe',
  description: 'A detailed description',
  prepTimeMinutes: 15,
  cookTimeMinutes: 30,
  totalTimeMinutes: 45,
  servings: 2,
  difficulty: 'medium',
  cuisines: ['French'],
  diets: ['vegan'],
  intolerances: ['gluten'],
  ingredients: [
    { id: 'i1', name: 'Butter', amount: 50, unit: 'g', originalAmount: 50 },
  ],
  steps: [
    { number: 1, description: 'Melt the butter.' },
  ],
  likes: 5,
  isLiked: false,
  author: { id: 'a1', name: 'Chef Bob' },
  source: 'community',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-02T00:00:00Z',
}

function wrapper({ children }: { children: React.ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('RecipeDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => mockRecipe,
    } as Response)
  })

  it('shows skeleton while loading', () => {
    render(<RecipeDetailPage id="1" />, { wrapper })
    expect(screen.getByLabelText('Loading recipe')).toBeInTheDocument()
  })

  it('renders recipe after loading', async () => {
    render(<RecipeDetailPage id="1" />, { wrapper })
    await waitFor(() => {
      expect(screen.getByText('Integration Test Recipe')).toBeInTheDocument()
    })
  })

  it('switches tabs', async () => {
    const user = userEvent.setup()
    render(<RecipeDetailPage id="1" />, { wrapper })
    await waitFor(() => screen.getByText('Integration Test Recipe'))
    await user.click(screen.getByRole('tab', { name: 'Ingredients' }))
    expect(screen.getByText('Butter')).toBeVisible()
  })

  it('shows 404 error state', async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
    } as Response)
    render(<RecipeDetailPage id="bad" />, { wrapper })
    await waitFor(() => {
      expect(screen.getByText('Recipe not found')).toBeInTheDocument()
    })
  })

  it('shows 403 error state', async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 403,
    } as Response)
    render(<RecipeDetailPage id="private" />, { wrapper })
    await waitFor(() => {
      expect(screen.getByText('This recipe is private')).toBeInTheDocument()
    })
  })
})
