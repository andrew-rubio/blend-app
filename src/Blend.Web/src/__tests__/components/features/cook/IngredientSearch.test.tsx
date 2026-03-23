import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import { IngredientSearch } from '@/components/features/cook/IngredientSearch'
import type { IngredientSearchResult } from '@/types'

vi.mock('@/hooks/useCookMode', () => ({
  useIngredientSearch: vi.fn(),
}))

import { useIngredientSearch } from '@/hooks/useCookMode'
const mockUseIngredientSearch = vi.mocked(useIngredientSearch)

const mockResults: IngredientSearchResult[] = [
  { id: 'ing-1', name: 'Garlic', category: 'Alliums' },
  { id: 'ing-2', name: 'Garam Masala', category: 'Spices' },
]

describe('IngredientSearch', () => {
  const mockOnAdd = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseIngredientSearch.mockReturnValue({ data: [], isLoading: false } as ReturnType<typeof useIngredientSearch>)
  })

  it('renders search input', () => {
    render(<IngredientSearch onAdd={mockOnAdd} />)
    expect(screen.getByTestId('ingredient-search-input')).toBeDefined()
  })

  it('does not show dropdown when query is short', () => {
    render(<IngredientSearch onAdd={mockOnAdd} />)
    expect(screen.queryByTestId('ingredient-search-dropdown')).toBeNull()
  })

  it('shows dropdown with results when query >= 2 chars', async () => {
    mockUseIngredientSearch.mockReturnValue({ data: mockResults, isLoading: false } as ReturnType<typeof useIngredientSearch>)
    render(<IngredientSearch onAdd={mockOnAdd} />)
    const input = screen.getByTestId('ingredient-search-input')
    fireEvent.change(input, { target: { value: 'ga' } })
    // After debounce
    await act(async () => {
      await new Promise((r) => setTimeout(r, 250))
    })
    expect(screen.getByTestId('ingredient-search-dropdown')).toBeDefined()
    expect(screen.getByText('Garlic')).toBeDefined()
  })

  it('shows empty message when no results', async () => {
    mockUseIngredientSearch.mockReturnValue({ data: [], isLoading: false } as ReturnType<typeof useIngredientSearch>)
    render(<IngredientSearch onAdd={mockOnAdd} />)
    const input = screen.getByTestId('ingredient-search-input')
    fireEvent.change(input, { target: { value: 'xx' } })
    await act(async () => {
      await new Promise((r) => setTimeout(r, 250))
    })
    expect(screen.getByTestId('ingredient-search-empty')).toBeDefined()
  })

  it('calls onAdd when ingredient option clicked', async () => {
    mockUseIngredientSearch.mockReturnValue({ data: mockResults, isLoading: false } as ReturnType<typeof useIngredientSearch>)
    render(<IngredientSearch onAdd={mockOnAdd} />)
    const input = screen.getByTestId('ingredient-search-input')
    fireEvent.change(input, { target: { value: 'ga' } })
    await act(async () => {
      await new Promise((r) => setTimeout(r, 250))
    })
    fireEvent.mouseDown(screen.getByTestId('ingredient-option-0'))
    expect(mockOnAdd).toHaveBeenCalledWith(mockResults[0])
  })

  it('clears input after selection', async () => {
    mockUseIngredientSearch.mockReturnValue({ data: mockResults, isLoading: false } as ReturnType<typeof useIngredientSearch>)
    render(<IngredientSearch onAdd={mockOnAdd} />)
    const input = screen.getByTestId('ingredient-search-input') as HTMLInputElement
    fireEvent.change(input, { target: { value: 'ga' } })
    await act(async () => {
      await new Promise((r) => setTimeout(r, 250))
    })
    fireEvent.mouseDown(screen.getByTestId('ingredient-option-0'))
    expect(input.value).toBe('')
  })

  it('disables input when disabled prop is true', () => {
    render(<IngredientSearch onAdd={mockOnAdd} disabled />)
    const input = screen.getByTestId('ingredient-search-input') as HTMLInputElement
    expect(input.disabled).toBe(true)
  })
})
