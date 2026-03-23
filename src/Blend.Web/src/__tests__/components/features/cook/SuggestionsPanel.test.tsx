import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SuggestionsPanel } from '@/components/features/cook/SuggestionsPanel'
import type { SessionSuggestionsResult } from '@/types'

vi.mock('@/hooks/useCookMode', () => ({
  useSuggestions: vi.fn(),
}))

import { useSuggestions } from '@/hooks/useCookMode'
const mockUseSuggestions = vi.mocked(useSuggestions)

const mockResult: SessionSuggestionsResult = {
  suggestions: [
    { ingredientId: 'ing-1', name: 'Basil', aggregateScore: 0.9, reason: 'Great with tomatoes' },
    { ingredientId: 'ing-2', name: 'Oregano', aggregateScore: 0.75, reason: 'Classic pairing' },
  ],
  kbUnavailable: false,
}

describe('SuggestionsPanel', () => {
  const mockOnAdd = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows loading skeletons when loading', () => {
    mockUseSuggestions.mockReturnValue({ data: undefined, isLoading: true } as ReturnType<typeof useSuggestions>)
    render(<SuggestionsPanel sessionId="s-1" onAdd={mockOnAdd} />)
    expect(screen.getByTestId('suggestions-loading')).toBeDefined()
    expect(screen.getByTestId('suggestion-skeleton-0')).toBeDefined()
  })

  it('shows unavailable message when kbUnavailable', () => {
    mockUseSuggestions.mockReturnValue({
      data: { suggestions: [], kbUnavailable: true },
      isLoading: false,
    } as ReturnType<typeof useSuggestions>)
    render(<SuggestionsPanel sessionId="s-1" onAdd={mockOnAdd} />)
    expect(screen.getByTestId('suggestions-unavailable')).toBeDefined()
  })

  it('shows empty message when no suggestions', () => {
    mockUseSuggestions.mockReturnValue({
      data: { suggestions: [], kbUnavailable: false },
      isLoading: false,
    } as ReturnType<typeof useSuggestions>)
    render(<SuggestionsPanel sessionId="s-1" onAdd={mockOnAdd} />)
    expect(screen.getByTestId('suggestions-empty')).toBeDefined()
  })

  it('renders suggestion items', () => {
    mockUseSuggestions.mockReturnValue({
      data: mockResult,
      isLoading: false,
    } as ReturnType<typeof useSuggestions>)
    render(<SuggestionsPanel sessionId="s-1" onAdd={mockOnAdd} />)
    expect(screen.getByText('Basil')).toBeDefined()
    expect(screen.getByText('Oregano')).toBeDefined()
    expect(screen.getByText('90%')).toBeDefined()
  })

  it('calls onAdd when suggestion clicked', () => {
    mockUseSuggestions.mockReturnValue({
      data: mockResult,
      isLoading: false,
    } as ReturnType<typeof useSuggestions>)
    render(<SuggestionsPanel sessionId="s-1" onAdd={mockOnAdd} />)
    fireEvent.click(screen.getByTestId('suggestion-ing-1'))
    expect(mockOnAdd).toHaveBeenCalledWith(mockResult.suggestions[0])
  })
})
