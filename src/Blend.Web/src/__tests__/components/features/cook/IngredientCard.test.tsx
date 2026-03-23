import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { IngredientCard } from '@/components/features/cook/IngredientCard'
import type { SessionIngredient } from '@/types'

const mockIngredient: SessionIngredient = {
  ingredientId: 'ing-1',
  name: 'Garlic',
  addedAt: '2024-01-01T00:00:00Z',
}

describe('IngredientCard', () => {
  const mockOnRemove = vi.fn()
  const mockOnDetail = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders ingredient name', () => {
    render(<IngredientCard ingredient={mockIngredient} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    expect(screen.getByText('Garlic')).toBeDefined()
  })

  it('renders initial letter avatar', () => {
    render(<IngredientCard ingredient={mockIngredient} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    expect(screen.getByText('G')).toBeDefined()
  })

  it('renders notes when present', () => {
    const withNotes: SessionIngredient = { ...mockIngredient, notes: 'finely chopped' }
    render(<IngredientCard ingredient={withNotes} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    expect(screen.getByText('finely chopped')).toBeDefined()
  })

  it('calls onDetail when body button clicked', () => {
    render(<IngredientCard ingredient={mockIngredient} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    fireEvent.click(screen.getByTestId('ingredient-card-body-ing-1'))
    expect(mockOnDetail).toHaveBeenCalledWith('ing-1')
  })

  it('calls onRemove when remove button clicked', () => {
    render(<IngredientCard ingredient={mockIngredient} onRemove={mockOnRemove} onDetail={mockOnDetail} />)
    fireEvent.click(screen.getByTestId('ingredient-card-remove-ing-1'))
    expect(mockOnRemove).toHaveBeenCalledWith('ing-1')
  })
})
