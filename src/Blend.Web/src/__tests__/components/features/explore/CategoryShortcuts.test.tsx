import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { CategoryShortcuts } from '@/components/features/explore/CategoryShortcuts'
import { useSearchStore } from '@/stores/searchStore'

const emptyFilters = { cuisines: [], diets: [], dishTypes: [], maxReadyTime: null }

describe('CategoryShortcuts', () => {
  beforeEach(() => {
    useSearchStore.setState({ filters: emptyFilters, query: '', isFilterPanelOpen: false })
  })

  it('renders category chips', () => {
    render(<CategoryShortcuts />)
    expect(screen.getByLabelText('🍝 Italian')).toBeDefined()
    expect(screen.getByLabelText('🍣 Japanese')).toBeDefined()
    expect(screen.getByLabelText('🌮 Mexican')).toBeDefined()
  })

  it('selects a cuisine category on click', () => {
    render(<CategoryShortcuts />)
    fireEvent.click(screen.getByLabelText('🍝 Italian'))
    expect(useSearchStore.getState().filters.cuisines).toContain('Italian')
  })

  it('deselects a cuisine category when clicked again', () => {
    useSearchStore.setState({ filters: { ...emptyFilters, cuisines: ['Italian'] } })
    render(<CategoryShortcuts />)
    fireEvent.click(screen.getByLabelText('🍝 Italian'))
    expect(useSearchStore.getState().filters.cuisines).not.toContain('Italian')
  })

  it('selects a dish type category on click', () => {
    render(<CategoryShortcuts />)
    fireEvent.click(screen.getByLabelText('🥗 Salad'))
    expect(useSearchStore.getState().filters.dishTypes).toContain('salad')
  })

  it('shows selected state on active category', () => {
    useSearchStore.setState({ filters: { ...emptyFilters, cuisines: ['Italian'] } })
    render(<CategoryShortcuts />)
    const chip = screen.getByLabelText('🍝 Italian')
    expect(chip).toHaveAttribute('aria-checked', 'true')
  })

  it('shows unselected state on inactive category', () => {
    render(<CategoryShortcuts />)
    const chip = screen.getByLabelText('🍝 Italian')
    expect(chip).toHaveAttribute('aria-checked', 'false')
  })

  it('calls onSelect callback when a category is clicked', () => {
    const handleSelect = vi.fn()
    render(<CategoryShortcuts onSelect={handleSelect} />)
    fireEvent.click(screen.getByLabelText('🍝 Italian'))
    expect(handleSelect).toHaveBeenCalledWith('Italian')
  })

  it('has section heading', () => {
    render(<CategoryShortcuts />)
    expect(screen.getByText('Browse by Category')).toBeDefined()
  })
})
