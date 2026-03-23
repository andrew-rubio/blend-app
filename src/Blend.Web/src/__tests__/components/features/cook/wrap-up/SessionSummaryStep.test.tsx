import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SessionSummaryStep } from '@/components/features/cook/wrap-up/SessionSummaryStep'
import type { CookingSession } from '@/types'

const mockSession: CookingSession = {
  id: 'session-1',
  userId: 'user-1',
  dishes: [
    {
      dishId: 'dish-1',
      name: 'Pasta Dish',
      ingredients: [
        { ingredientId: 'ing-tomato', name: 'Tomato', addedAt: '2024-01-01T00:00:00Z' },
        { ingredientId: 'ing-basil', name: 'Basil', addedAt: '2024-01-01T00:00:00Z' },
      ],
      notes: 'Very tasty!',
    },
  ],
  addedIngredients: [
    { ingredientId: 'ing-garlic', name: 'Garlic', addedAt: '2024-01-01T00:00:00Z' },
  ],
  status: 'Completed',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

describe('SessionSummaryStep', () => {
  it('renders session summary heading', () => {
    render(<SessionSummaryStep session={mockSession} onNext={vi.fn()} />)
    expect(screen.getByText('Session Summary')).toBeDefined()
  })

  it('shows all dish names', () => {
    render(<SessionSummaryStep session={mockSession} onNext={vi.fn()} />)
    expect(screen.getByText('Pasta Dish')).toBeDefined()
  })

  it('shows ingredients in each dish', () => {
    render(<SessionSummaryStep session={mockSession} onNext={vi.fn()} />)
    expect(screen.getByText('• Tomato')).toBeDefined()
    expect(screen.getByText('• Basil')).toBeDefined()
  })

  it('shows session-level ingredients', () => {
    render(<SessionSummaryStep session={mockSession} onNext={vi.fn()} />)
    expect(screen.getByText('Additional Ingredients')).toBeDefined()
    expect(screen.getByText('• Garlic')).toBeDefined()
  })

  it('shows dish notes', () => {
    render(<SessionSummaryStep session={mockSession} onNext={vi.fn()} />)
    expect(screen.getByText(/Very tasty!/)).toBeDefined()
  })

  it('calls onNext when Continue is clicked', () => {
    const onNext = vi.fn()
    render(<SessionSummaryStep session={mockSession} onNext={onNext} />)
    fireEvent.click(screen.getByTestId('summary-next-button'))
    expect(onNext).toHaveBeenCalledOnce()
  })

  it('shows empty message when no ingredients', () => {
    const emptySession: CookingSession = {
      ...mockSession,
      dishes: [{ dishId: 'dish-1', name: 'Empty Dish', ingredients: [] }],
      addedIngredients: [],
    }
    render(<SessionSummaryStep session={emptySession} onNext={vi.fn()} />)
    expect(screen.getByText('No ingredients were recorded in this session.')).toBeDefined()
  })
})
