import React from 'react'
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { DirectionsTab } from '@/components/features/recipe/DirectionsTab'
import type { Recipe } from '@/types'

const baseRecipe: Recipe = {
  id: '1',
  title: 'Test Recipe',
  cuisines: [],
  dishTypes: [],
  diets: [],
  intolerances: [],
  servings: 4,
  ingredients: [],
  steps: [
    { number: 1, step: 'Boil water in a large pot.' },
    { number: 2, step: 'Add pasta and cook for 10 minutes.' },
    { number: 3, step: 'Drain and serve.', imageUrl: 'https://example.com/step3.jpg' },
  ],
  dataSource: 'Spoonacular',
  likeCount: 0,
}

describe('DirectionsTab', () => {
  it('renders all step texts', () => {
    render(<DirectionsTab recipe={baseRecipe} />)
    expect(screen.getByText('Boil water in a large pot.')).toBeDefined()
    expect(screen.getByText('Add pasta and cook for 10 minutes.')).toBeDefined()
    expect(screen.getByText('Drain and serve.')).toBeDefined()
  })

  it('renders step number bubbles', () => {
    render(<DirectionsTab recipe={baseRecipe} />)
    // Step numbers are rendered in their own divs
    const listItems = screen.getAllByRole('listitem')
    expect(listItems.length).toBe(3)
  })

  it('renders step image when imageUrl is provided', () => {
    render(<DirectionsTab recipe={baseRecipe} />)
    const img = screen.getByAltText('Step 3')
    expect(img).toBeDefined()
    expect(img).toHaveAttribute('src', 'https://example.com/step3.jpg')
  })

  it('does not render images for steps without imageUrl', () => {
    render(<DirectionsTab recipe={baseRecipe} />)
    expect(screen.queryByAltText('Step 1')).toBeNull()
    expect(screen.queryByAltText('Step 2')).toBeNull()
  })

  it('renders the ordered list with aria-label', () => {
    render(<DirectionsTab recipe={baseRecipe} />)
    expect(screen.getByLabelText('Directions')).toBeDefined()
  })

  it('shows empty state when no steps', () => {
    render(<DirectionsTab recipe={{ ...baseRecipe, steps: [] }} />)
    expect(screen.getByText('No directions available.')).toBeDefined()
  })
})
