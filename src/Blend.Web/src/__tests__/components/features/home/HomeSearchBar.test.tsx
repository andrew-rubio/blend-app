import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { HomeSearchBar } from '@/components/features/home/HomeSearchBar'

const mockPush = vi.fn()

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

describe('HomeSearchBar', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders a search input', () => {
    render(<HomeSearchBar />)
    expect(screen.getByRole('searchbox')).toBeDefined()
  })

  it('navigates to explore on click', () => {
    render(<HomeSearchBar />)
    const input = screen.getByRole('searchbox')
    fireEvent.click(input)
    expect(mockPush).toHaveBeenCalledWith('/explore?focus=search')
  })

  it('navigates to explore on focus', () => {
    render(<HomeSearchBar />)
    const input = screen.getByRole('searchbox')
    fireEvent.focus(input)
    expect(mockPush).toHaveBeenCalledWith('/explore?focus=search')
  })

  it('shows the initial placeholder when provided', () => {
    render(<HomeSearchBar initialPlaceholder="Try chicken..." />)
    const input = screen.getByRole('searchbox')
    expect(input).toBeDefined()
  })

  it('has a search role container', () => {
    render(<HomeSearchBar />)
    expect(screen.getByRole('search')).toBeDefined()
  })

  it('has accessible aria-label', () => {
    render(<HomeSearchBar />)
    const input = screen.getByLabelText('Search for recipes')
    expect(input).toBeDefined()
  })
})
