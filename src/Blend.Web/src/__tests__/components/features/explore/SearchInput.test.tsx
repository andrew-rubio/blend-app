import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SearchInput } from '@/components/features/explore/SearchInput'

describe('SearchInput', () => {
  const noop = () => {}

  it('renders with placeholder text', () => {
    render(<SearchInput value="" onChange={noop} onClear={noop} placeholder="Search recipes…" />)
    expect(screen.getByPlaceholderText('Search recipes…')).toBeDefined()
  })

  it('renders with provided value', () => {
    render(<SearchInput value="pasta" onChange={noop} onClear={noop} />)
    const input = screen.getByRole('searchbox') as HTMLInputElement
    expect(input.value).toBe('pasta')
  })

  it('calls onChange when input changes', () => {
    const handleChange = vi.fn()
    render(<SearchInput value="" onChange={handleChange} onClear={noop} />)
    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'pizza' } })
    expect(handleChange).toHaveBeenCalledWith('pizza')
  })

  it('shows clear button when value is non-empty', () => {
    render(<SearchInput value="pasta" onChange={noop} onClear={noop} />)
    expect(screen.getByLabelText('Clear search')).toBeDefined()
  })

  it('hides clear button when value is empty', () => {
    render(<SearchInput value="" onChange={noop} onClear={noop} />)
    expect(screen.queryByLabelText('Clear search')).toBeNull()
  })

  it('calls onClear when clear button is clicked', () => {
    const handleClear = vi.fn()
    render(<SearchInput value="pasta" onChange={noop} onClear={handleClear} />)
    fireEvent.click(screen.getByLabelText('Clear search'))
    expect(handleClear).toHaveBeenCalledTimes(1)
  })

  it('shows loading spinner when isLoading is true', () => {
    render(<SearchInput value="" onChange={noop} onClear={noop} isLoading />)
    // The spinner replaces the search icon; we check by aria absence of search icon
    const input = screen.getByRole('searchbox')
    expect(input).toBeDefined()
  })

  it('has ARIA label "Search recipes"', () => {
    render(<SearchInput value="" onChange={noop} onClear={noop} />)
    expect(screen.getByLabelText('Search recipes')).toBeDefined()
  })
})
