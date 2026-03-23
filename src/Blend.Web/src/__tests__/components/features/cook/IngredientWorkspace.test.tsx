import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { IngredientWorkspace } from '@/components/features/cook/IngredientWorkspace'
import type { SessionIngredient } from '@/types'

const ingredients: SessionIngredient[] = [
  { ingredientId: 'ing-1', name: 'Garlic', addedAt: '2024-01-01T00:00:00Z' },
  { ingredientId: 'ing-2', name: 'Onion', addedAt: '2024-01-01T00:00:00Z' },
]

describe('IngredientWorkspace', () => {
  const mockOnRemove = vi.fn()
  const mockOnDetail = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders empty state when no ingredients', () => {
    render(<IngredientWorkspace ingredients={[]} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    expect(screen.getByTestId('ingredient-workspace-empty')).toBeDefined()
    expect(screen.getByText('No ingredients added yet')).toBeDefined()
  })

  it('renders all ingredient cards', () => {
    render(<IngredientWorkspace ingredients={ingredients} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    expect(screen.getByTestId('ingredient-workspace')).toBeDefined()
    expect(screen.getByText('Garlic')).toBeDefined()
    expect(screen.getByText('Onion')).toBeDefined()
  })

  it('passes dishId to onRemove', () => {
    render(<IngredientWorkspace ingredients={ingredients} onRemove={mockOnRemove} onDetail={mockOnDetail} dishId="dish-1" />)
    fireEvent.click(screen.getByTestId('ingredient-card-remove-ing-1'))
    expect(mockOnRemove).toHaveBeenCalledWith('ing-1', 'dish-1')
  })

  it('calls onDetail with ingredientId', () => {
    render(<IngredientWorkspace ingredients={ingredients} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    fireEvent.click(screen.getByTestId('ingredient-card-body-ing-1'))
    expect(mockOnDetail).toHaveBeenCalledWith('ing-1')
  })
})
