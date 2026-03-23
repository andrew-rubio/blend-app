import React from 'react'
import { describe, it, expect } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { IngredientsTab } from '@/components/features/recipe/IngredientsTab'
import type { Recipe } from '@/types'

const baseRecipe: Recipe = {
  id: '1',
  title: 'Test Recipe',
  cuisines: [],
  dishTypes: [],
  diets: [],
  intolerances: [],
  servings: 4,
  ingredients: [
    { id: 'i1', name: 'pasta', amount: 200, unit: 'g' },
    { id: 'i2', name: 'egg', amount: 2, unit: '' },
    { id: 'i3', name: 'parmesan', amount: 50, unit: 'g' },
  ],
  steps: [],
  dataSource: 'Spoonacular',
  likeCount: 0,
}

describe('IngredientsTab', () => {
  it('renders all ingredient names', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    expect(screen.getByText('pasta')).toBeDefined()
    expect(screen.getByText('egg')).toBeDefined()
    expect(screen.getByText('parmesan')).toBeDefined()
  })

  it('renders initial amounts for base servings', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    // pasta: 200g
    expect(screen.getByText(/200/)).toBeDefined()
  })

  it('renders the current serving count (initial)', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    expect(screen.getByLabelText('4 servings')).toBeDefined()
  })

  it('increments servings when + is clicked', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    fireEvent.click(screen.getByLabelText('Increase servings'))
    expect(screen.getByLabelText('5 servings')).toBeDefined()
  })

  it('decrements servings when − is clicked', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    fireEvent.click(screen.getByLabelText('Decrease servings'))
    expect(screen.getByLabelText('3 servings')).toBeDefined()
  })

  it('disables decrement button at 1 serving', () => {
    render(<IngredientsTab recipe={{ ...baseRecipe, servings: 1 }} />)
    const decBtn = screen.getByLabelText('Decrease servings')
    expect(decBtn).toHaveAttribute('disabled')
  })

  it('does not go below 1 serving', () => {
    render(<IngredientsTab recipe={{ ...baseRecipe, servings: 1 }} />)
    fireEvent.click(screen.getByLabelText('Decrease servings'))
    expect(screen.getByLabelText('1 servings')).toBeDefined()
  })

  it('scales ingredient amounts when servings change', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    // Initial: 200g pasta for 4 servings
    expect(screen.getByText(/200/)).toBeDefined()
    // Increase to 8 servings (double)
    fireEvent.click(screen.getByLabelText('Increase servings'))
    fireEvent.click(screen.getByLabelText('Increase servings'))
    fireEvent.click(screen.getByLabelText('Increase servings'))
    fireEvent.click(screen.getByLabelText('Increase servings'))
    // 200g * 8/4 = 400g
    expect(screen.getByText(/400/)).toBeDefined()
  })

  it('shows the serving adjuster with aria-label', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    expect(screen.getByLabelText('Serving adjuster')).toBeDefined()
  })

  it('renders an accessible ingredients list', () => {
    render(<IngredientsTab recipe={baseRecipe} />)
    expect(screen.getByLabelText('Ingredients')).toBeDefined()
  })
})
