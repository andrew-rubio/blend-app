import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { PairingFeedbackStep } from '@/components/features/cook/wrap-up/PairingFeedbackStep'
import type { CookingSession, PairingFeedbackItem } from '@/types'

const mockSession: CookingSession = {
  id: 'session-1',
  userId: 'user-1',
  dishes: [
    {
      dishId: 'dish-1',
      name: 'Main Dish',
      ingredients: [
        { ingredientId: 'ing-tomato', name: 'Tomato', addedAt: '2024-01-01T00:00:00Z' },
        { ingredientId: 'ing-basil', name: 'Basil', addedAt: '2024-01-01T00:00:00Z' },
      ],
    },
  ],
  addedIngredients: [],
  status: 'Completed',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

describe('PairingFeedbackStep', () => {
  const defaultProps = {
    session: mockSession,
    feedbackItems: [] as PairingFeedbackItem[],
    onRate: vi.fn(),
    onNext: vi.fn(),
    onSkip: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders heading', () => {
    render(<PairingFeedbackStep {...defaultProps} />)
    expect(screen.getByText('Pairing Feedback')).toBeDefined()
  })

  it('shows ingredient pairs from dishes', () => {
    render(<PairingFeedbackStep {...defaultProps} />)
    expect(screen.getByText('Tomato + Basil')).toBeDefined()
  })

  it('renders star buttons for each pair', () => {
    render(<PairingFeedbackStep {...defaultProps} />)
    // 5 stars for the single pair
    const stars = screen.getAllByLabelText(/star/i)
    expect(stars.length).toBe(5)
  })

  it('calls onRate when a star is clicked', () => {
    const onRate = vi.fn()
    render(<PairingFeedbackStep {...defaultProps} onRate={onRate} />)
    fireEvent.click(screen.getByTestId('star-ing-tomato-ing-basil-4'))
    expect(onRate).toHaveBeenCalledOnce()
    const [id1, id2, rating] = onRate.mock.calls[0]
    expect(id1).toBe('ing-tomato')
    expect(id2).toBe('ing-basil')
    expect(rating).toBe(4)
  })

  it('calls onSkip when Skip is clicked', () => {
    const onSkip = vi.fn()
    render(<PairingFeedbackStep {...defaultProps} onSkip={onSkip} />)
    fireEvent.click(screen.getByTestId('feedback-skip-button'))
    expect(onSkip).toHaveBeenCalledOnce()
  })

  it('calls onNext when Continue is clicked', () => {
    const onNext = vi.fn()
    render(<PairingFeedbackStep {...defaultProps} onNext={onNext} />)
    fireEvent.click(screen.getByTestId('feedback-next-button'))
    expect(onNext).toHaveBeenCalledOnce()
  })

  it('shows rated count when items are rated', () => {
    const feedbackItems: PairingFeedbackItem[] = [
      { ingredientId1: 'ing-tomato', ingredientId2: 'ing-basil', rating: 5 },
    ]
    render(<PairingFeedbackStep {...defaultProps} feedbackItems={feedbackItems} />)
    expect(screen.getByTestId('rated-count')).toBeDefined()
    expect(screen.getByText(/1 of 1 pair rated/)).toBeDefined()
  })

  it('shows empty state when no pairs exist', () => {
    const emptySession: CookingSession = {
      ...mockSession,
      dishes: [{ dishId: 'd1', name: 'Empty', ingredients: [] }],
      addedIngredients: [],
    }
    render(<PairingFeedbackStep {...defaultProps} session={emptySession} />)
    expect(screen.getByText('No ingredient pairs to rate in this session.')).toBeDefined()
  })

  it('shows Submit & Continue label when pairs are rated', () => {
    const feedbackItems: PairingFeedbackItem[] = [
      { ingredientId1: 'ing-tomato', ingredientId2: 'ing-basil', rating: 4 },
    ]
    render(<PairingFeedbackStep {...defaultProps} feedbackItems={feedbackItems} />)
    expect(screen.getByText('Submit & Continue')).toBeDefined()
  })
})
