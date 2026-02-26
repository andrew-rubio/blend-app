import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { RecipeTabs } from '../RecipeTabs'
import type { Recipe } from '@/types/recipe'

const mockRecipe: Recipe = {
  id: '1',
  title: 'Test Recipe',
  description: 'A delicious test recipe',
  prepTimeMinutes: 10,
  cookTimeMinutes: 20,
  totalTimeMinutes: 30,
  servings: 4,
  difficulty: 'easy',
  cuisines: ['Italian'],
  diets: ['vegetarian'],
  intolerances: [],
  ingredients: [
    { id: 'i1', name: 'Flour', amount: 2, unit: 'cups', originalAmount: 2 },
  ],
  steps: [
    { number: 1, description: 'Mix the ingredients together.' },
  ],
  likes: 10,
  isLiked: false,
  author: { id: 'a1', name: 'Chef Alice' },
  source: 'community',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-02T00:00:00Z',
}

describe('RecipeTabs', () => {
  it('renders all three tabs', () => {
    render(<RecipeTabs recipe={mockRecipe} />)
    expect(screen.getByRole('tab', { name: 'Overview' })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Ingredients' })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Directions' })).toBeInTheDocument()
  })

  it('shows overview content by default', () => {
    render(<RecipeTabs recipe={mockRecipe} />)
    expect(screen.getByText('A delicious test recipe')).toBeVisible()
  })

  it('switches to ingredients tab on click', async () => {
    const user = userEvent.setup()
    render(<RecipeTabs recipe={mockRecipe} />)
    await user.click(screen.getByRole('tab', { name: 'Ingredients' }))
    expect(screen.getByRole('tab', { name: 'Ingredients' })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByText('Flour')).toBeVisible()
  })

  it('switches to directions tab on click', async () => {
    const user = userEvent.setup()
    render(<RecipeTabs recipe={mockRecipe} />)
    await user.click(screen.getByRole('tab', { name: 'Directions' }))
    expect(screen.getByRole('tab', { name: 'Directions' })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByText('Mix the ingredients together.')).toBeVisible()
  })
})
