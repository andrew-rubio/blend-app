import React from 'react'
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { OverviewTab } from '@/components/features/recipe/OverviewTab'
import type { Recipe } from '@/types'

const baseRecipe: Recipe = {
  id: '1',
  title: 'Test Recipe',
  description: 'A delicious test recipe.',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  diets: ['gluten free', 'vegetarian'],
  intolerances: ['peanut'],
  servings: 4,
  readyInMinutes: 45,
  prepTimeMinutes: 15,
  cookTimeMinutes: 30,
  difficulty: 'Easy',
  ingredients: [],
  steps: [],
  dataSource: 'Spoonacular',
  likeCount: 10,
}

describe('OverviewTab', () => {
  it('renders the recipe description', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('A delicious test recipe.')).toBeDefined()
  })

  it('renders prep time stat', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Prep')).toBeDefined()
    expect(screen.getByText('15 min')).toBeDefined()
  })

  it('renders cook time stat', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Cook')).toBeDefined()
    expect(screen.getByText('30 min')).toBeDefined()
  })

  it('renders total time from readyInMinutes', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Total')).toBeDefined()
    expect(screen.getByText('45 min')).toBeDefined()
  })

  it('renders servings stat', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Servings')).toBeDefined()
    expect(screen.getByText('4')).toBeDefined()
  })

  it('renders difficulty stat', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Difficulty')).toBeDefined()
    expect(screen.getByText('Easy')).toBeDefined()
  })

  it('renders diet badges', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('gluten free')).toBeDefined()
    expect(screen.getByText('vegetarian')).toBeDefined()
  })

  it('renders intolerance badges', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('peanut')).toBeDefined()
  })

  it('shows Diets heading when diets are present', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Diets')).toBeDefined()
  })

  it('shows Free From heading when intolerances are present', () => {
    render(<OverviewTab recipe={baseRecipe} />)
    expect(screen.getByText('Free From')).toBeDefined()
  })

  it('does not show Diets section when no diets', () => {
    render(<OverviewTab recipe={{ ...baseRecipe, diets: [] }} />)
    expect(screen.queryByText('Diets')).toBeNull()
  })

  it('does not show Free From section when no intolerances', () => {
    render(<OverviewTab recipe={{ ...baseRecipe, intolerances: [] }} />)
    expect(screen.queryByText('Free From')).toBeNull()
  })

  it('does not render description when absent', () => {
    const { container } = render(<OverviewTab recipe={{ ...baseRecipe, description: undefined }} />)
    expect(container.querySelector('p')).toBeNull()
  })

  it('renders photo gallery when multiple photos', () => {
    render(
      <OverviewTab
        recipe={{
          ...baseRecipe,
          photos: ['https://example.com/1.jpg', 'https://example.com/2.jpg', 'https://example.com/3.jpg'],
        }}
      />
    )
    expect(screen.getByText('Photos')).toBeDefined()
    expect(screen.getAllByRole('img').length).toBeGreaterThan(0)
  })

  it('does not render gallery when only one photo', () => {
    render(
      <OverviewTab
        recipe={{ ...baseRecipe, photos: ['https://example.com/1.jpg'] }}
      />
    )
    expect(screen.queryByText('Photos')).toBeNull()
  })
})
