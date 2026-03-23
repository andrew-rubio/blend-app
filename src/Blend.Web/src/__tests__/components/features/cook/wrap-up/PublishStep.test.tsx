import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { PublishStep } from '@/components/features/cook/wrap-up/PublishStep'
import type { CookingSession } from '@/types'

const mockSession: CookingSession = {
  id: 'session-1',
  userId: 'user-1',
  dishes: [
    {
      dishId: 'dish-1',
      name: 'Main Dish',
      ingredients: [
        { ingredientId: 'ing-tomato', name: 'Tomato', addedAt: '2024-01-01T00:00:00Z' },
      ],
    },
  ],
  addedIngredients: [],
  status: 'Completed',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

const emptyForm = {
  title: '',
  description: '',
  directions: [] as { stepNumber: number; text: string }[],
  cuisineType: '',
  tags: [] as string[],
  servings: 0,
  prepTime: 0,
  cookTime: 0,
}

describe('PublishStep', () => {
  const defaultProps = {
    session: mockSession,
    shouldPublish: false,
    form: emptyForm,
    onTogglePublish: vi.fn(),
    onFieldChange: vi.fn(),
    onAddDirection: vi.fn(),
    onUpdateDirection: vi.fn(),
    onRemoveDirection: vi.fn(),
    onNext: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders heading', () => {
    render(<PublishStep {...defaultProps} />)
    expect(screen.getByText('Publish Recipe')).toBeDefined()
  })

  it('shows publish toggle', () => {
    render(<PublishStep {...defaultProps} />)
    expect(screen.getByTestId('publish-toggle')).toBeDefined()
  })

  it('hides publish form when toggle is off', () => {
    render(<PublishStep {...defaultProps} shouldPublish={false} />)
    expect(screen.queryByTestId('publish-form')).toBeNull()
  })

  it('shows publish form when toggle is on', () => {
    render(<PublishStep {...defaultProps} shouldPublish={true} />)
    expect(screen.getByTestId('publish-form')).toBeDefined()
  })

  it('calls onTogglePublish when toggle is clicked', () => {
    const onTogglePublish = vi.fn()
    render(<PublishStep {...defaultProps} onTogglePublish={onTogglePublish} />)
    fireEvent.click(screen.getByTestId('publish-toggle'))
    expect(onTogglePublish).toHaveBeenCalledWith(true)
  })

  it('calls onNext(false) when Finish is clicked without publishing', () => {
    const onNext = vi.fn()
    render(<PublishStep {...defaultProps} onNext={onNext} shouldPublish={false} />)
    fireEvent.click(screen.getByTestId('publish-next-button'))
    expect(onNext).toHaveBeenCalledWith(false)
  })

  it('calls onNext(true) when Skip is clicked', () => {
    const onNext = vi.fn()
    render(<PublishStep {...defaultProps} onNext={onNext} />)
    fireEvent.click(screen.getByTestId('publish-skip-button'))
    expect(onNext).toHaveBeenCalledWith(true)
  })

  it('calls onAddDirection when Add step is clicked', () => {
    const onAddDirection = vi.fn()
    render(<PublishStep {...defaultProps} shouldPublish={true} onAddDirection={onAddDirection} />)
    fireEvent.click(screen.getByTestId('add-direction-button'))
    expect(onAddDirection).toHaveBeenCalledOnce()
  })

  it('shows pre-populated session ingredients', () => {
    render(<PublishStep {...defaultProps} shouldPublish={true} />)
    expect(screen.getByText('• Tomato')).toBeDefined()
  })

  it('shows publish error when provided', () => {
    render(
      <PublishStep
        {...defaultProps}
        shouldPublish={true}
        publishError="Failed to publish"
      />,
    )
    expect(screen.getByTestId('publish-error')).toBeDefined()
    expect(screen.getByText('Failed to publish')).toBeDefined()
  })

  it('disables Publish button when form is invalid and shouldPublish', () => {
    render(<PublishStep {...defaultProps} shouldPublish={true} form={{ ...emptyForm, title: '' }} />)
    const button = screen.getByTestId('publish-next-button') as HTMLButtonElement
    expect(button.disabled).toBe(true)
  })

  it('enables Publish button when form is valid', () => {
    const validForm = {
      ...emptyForm,
      title: 'My Recipe',
      directions: [{ stepNumber: 1, text: 'Cook it.' }],
    }
    render(<PublishStep {...defaultProps} shouldPublish={true} form={validForm} />)
    const button = screen.getByTestId('publish-next-button') as HTMLButtonElement
    expect(button.disabled).toBe(false)
  })
})
