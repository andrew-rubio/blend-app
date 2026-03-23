import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { RecipeEditForm } from '@/components/features/recipe/RecipeEditForm'
import type { Recipe } from '@/types'

const mockRecipe: Recipe = {
  id: 'recipe-1',
  title: 'Tomato Pasta',
  description: 'A delicious pasta dish',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  diets: [],
  intolerances: [],
  servings: 4,
  prepTimeMinutes: 15,
  cookTimeMinutes: 30,
  ingredients: [
    { id: 'ing-1', name: 'Tomato', amount: 2, unit: 'cups' },
    { id: 'ing-2', name: 'Pasta', amount: 200, unit: 'g' },
  ],
  steps: [
    { number: 1, step: 'Boil water.' },
    { number: 2, step: 'Add pasta.' },
  ],
  dataSource: 'Community',
  likeCount: 5,
}

describe('RecipeEditForm', () => {
  const defaultProps = {
    recipe: mockRecipe,
    onSave: vi.fn(),
    onCancel: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the form', () => {
    render(<RecipeEditForm {...defaultProps} />)
    expect(screen.getByTestId('recipe-edit-form')).toBeDefined()
  })

  it('pre-populates title from recipe', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const titleInput = screen.getByTestId('edit-title-input') as HTMLInputElement
    expect(titleInput.value).toBe('Tomato Pasta')
  })

  it('pre-populates description from recipe', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const descInput = screen.getByTestId('edit-description-input') as HTMLTextAreaElement
    expect(descInput.value).toBe('A delicious pasta dish')
  })

  it('pre-populates ingredients from recipe', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const firstIngName = screen.getByTestId('ingredient-name-0') as HTMLInputElement
    expect(firstIngName.value).toBe('Tomato')
  })

  it('pre-populates directions from recipe', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const firstDir = screen.getByTestId('direction-input-0') as HTMLInputElement
    expect(firstDir.value).toBe('Boil water.')
  })

  it('calls onCancel when Cancel is clicked', () => {
    const onCancel = vi.fn()
    render(<RecipeEditForm {...defaultProps} onCancel={onCancel} />)
    fireEvent.click(screen.getByTestId('cancel-edit-button'))
    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('shows title error when submitting with empty title', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const titleInput = screen.getByTestId('edit-title-input') as HTMLInputElement
    fireEvent.change(titleInput, { target: { value: '' } })
    fireEvent.submit(screen.getByTestId('recipe-edit-form'))
    expect(screen.getByText('Title is required.')).toBeDefined()
  })

  it('calls onSave with correct payload when form is valid', () => {
    const onSave = vi.fn()
    render(<RecipeEditForm {...defaultProps} onSave={onSave} />)
    fireEvent.submit(screen.getByTestId('recipe-edit-form'))
    expect(onSave).toHaveBeenCalledOnce()
    const payload = onSave.mock.calls[0][0]
    expect(payload.title).toBe('Tomato Pasta')
    expect(payload.ingredients).toHaveLength(2)
    expect(payload.directions).toHaveLength(2)
  })

  it('shows save error when provided', () => {
    render(<RecipeEditForm {...defaultProps} saveError="Failed to save" />)
    expect(screen.getByTestId('save-error')).toBeDefined()
    expect(screen.getByText('Failed to save')).toBeDefined()
  })

  it('disables save button when saving', () => {
    render(<RecipeEditForm {...defaultProps} isSaving={true} />)
    const button = screen.getByTestId('save-edit-button') as HTMLButtonElement
    expect(button.disabled).toBe(true)
    expect(button.textContent).toBe('Saving…')
  })

  it('adds a new ingredient when Add ingredient is clicked', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const before = screen.getAllByTestId(/ingredient-name-/).length
    fireEvent.click(screen.getByTestId('add-ingredient-button'))
    const after = screen.getAllByTestId(/ingredient-name-/).length
    expect(after).toBe(before + 1)
  })

  it('removes an ingredient when Remove is clicked', () => {
    render(<RecipeEditForm {...defaultProps} />)
    const before = screen.getAllByTestId(/ingredient-name-/).length
    fireEvent.click(screen.getByTestId('remove-ingredient-0'))
    const after = screen.getAllByTestId(/ingredient-name-/).length
    expect(after).toBe(before - 1)
  })
})
