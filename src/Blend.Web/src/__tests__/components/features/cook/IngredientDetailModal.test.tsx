import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { IngredientDetailModal } from '@/components/features/cook/IngredientDetailModal'
import type { IngredientDetailResult } from '@/types'

vi.mock('@/hooks/useCookMode', () => ({
  useIngredientDetail: vi.fn(),
}))

import { useIngredientDetail } from '@/hooks/useCookMode'
const mockUseIngredientDetail = vi.mocked(useIngredientDetail)

const mockDetail: IngredientDetailResult = {
  ingredientId: 'ing-1',
  name: 'Garlic',
  category: 'Alliums',
  flavourProfile: 'Pungent, savory',
  substitutes: ['Shallots', 'Onion'],
  whyItPairs: 'Enhances umami',
  nutritionSummary: 'Low calorie, rich in antioxidants',
}

describe('IngredientDetailModal', () => {
  const mockOnClose = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders loading state', () => {
    mockUseIngredientDetail.mockReturnValue({ data: undefined, isLoading: true, error: null } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    expect(screen.getByTestId('ingredient-detail-loading')).toBeDefined()
  })

  it('renders error state', () => {
    mockUseIngredientDetail.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { message: 'Not found' },
    } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    expect(screen.getByTestId('ingredient-detail-error')).toBeDefined()
    expect(screen.getByText('Knowledge Base unavailable')).toBeDefined()
  })

  it('renders ingredient detail content', () => {
    mockUseIngredientDetail.mockReturnValue({ data: mockDetail, isLoading: false, error: null } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    expect(screen.getByTestId('ingredient-detail-content')).toBeDefined()
    expect(screen.getByText('Garlic')).toBeDefined()
    expect(screen.getByText('Alliums')).toBeDefined()
    expect(screen.getByText('Pungent, savory')).toBeDefined()
    expect(screen.getByText('Shallots')).toBeDefined()
    expect(screen.getByText('Onion')).toBeDefined()
  })

  it('calls onClose when close button clicked', () => {
    mockUseIngredientDetail.mockReturnValue({ data: mockDetail, isLoading: false, error: null } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    fireEvent.click(screen.getByTestId('ingredient-detail-close'))
    expect(mockOnClose).toHaveBeenCalled()
  })

  it('calls onClose when backdrop clicked', () => {
    mockUseIngredientDetail.mockReturnValue({ data: mockDetail, isLoading: false, error: null } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    fireEvent.click(screen.getByTestId('ingredient-detail-backdrop'))
    expect(mockOnClose).toHaveBeenCalled()
  })

  it('calls onClose when Escape key pressed', () => {
    mockUseIngredientDetail.mockReturnValue({ data: mockDetail, isLoading: false, error: null } as ReturnType<typeof useIngredientDetail>)
    render(<IngredientDetailModal sessionId="s-1" ingredientId="ing-1" onClose={mockOnClose} />)
    fireEvent.keyDown(document, { key: 'Escape' })
    expect(mockOnClose).toHaveBeenCalled()
  })
})
