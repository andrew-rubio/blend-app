import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { IngredientsTab } from '../IngredientsTab'
import type { Recipe } from '@/types/recipe'

const mockRecipe: Recipe = {
  id: '1',
  title: 'Test Recipe',
  description: 'Test',
  prepTimeMinutes: 10,
  cookTimeMinutes: 20,
  totalTimeMinutes: 30,
  servings: 4,
  difficulty: 'easy',
  cuisines: [],
  diets: [],
  intolerances: [],
  ingredients: [
    { id: 'i1', name: 'Flour', amount: 2, unit: 'cups', originalAmount: 2 },
    { id: 'i2', name: 'Sugar', amount: 1, unit: 'cup', originalAmount: 1 },
  ],
  steps: [],
  likes: 0,
  isLiked: false,
  author: { id: 'a1', name: 'Alice' },
  source: 'community',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

describe('IngredientsTab', () => {
  it('renders ingredient names', () => {
    render(<IngredientsTab recipe={mockRecipe} />)
    expect(screen.getByText('Flour')).toBeInTheDocument()
    expect(screen.getByText('Sugar')).toBeInTheDocument()
  })

  it('recalculates ingredient amounts when servings are doubled', async () => {
    const user = userEvent.setup()
    render(<IngredientsTab recipe={mockRecipe} />)
    // Initial: 2 cups flour, 1 cup sugar for 4 servings
    expect(screen.getByText('2 cups')).toBeInTheDocument()
    // Increase servings twice (4 -> 8)
    await user.click(screen.getByRole('button', { name: 'Increase servings' }))
    await user.click(screen.getByRole('button', { name: 'Increase servings' }))
    await user.click(screen.getByRole('button', { name: 'Increase servings' }))
    await user.click(screen.getByRole('button', { name: 'Increase servings' }))
    // 8 servings = double: flour should be 4 cups
    expect(screen.getByText('4 cups')).toBeInTheDocument()
  })
})
