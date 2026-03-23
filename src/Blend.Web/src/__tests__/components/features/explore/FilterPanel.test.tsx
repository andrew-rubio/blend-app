import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FilterPanel, FilterButton } from '@/components/features/explore/FilterPanel'
import { useSearchStore } from '@/stores/searchStore'
import type { SearchFilters } from '@/types'

// Mock preference hooks
vi.mock('@/hooks/usePreferences', () => ({
  useCuisines: () => ({ data: ['Italian', 'Mexican', 'Japanese'] }),
  useDishTypes: () => ({ data: ['main course', 'dessert', 'soup'] }),
  useDiets: () => ({ data: ['vegan', 'vegetarian'] }),
}))

const emptyFilters: SearchFilters = {
  cuisines: [],
  diets: [],
  dishTypes: [],
  maxReadyTime: null,
}

describe('FilterPanel', () => {
  beforeEach(() => {
    useSearchStore.setState({ filters: emptyFilters, isFilterPanelOpen: false, query: '' })
  })

  it('renders filter sections for cuisines, diets, and dish types', () => {
    render(<FilterPanel onClose={() => {}} />)
    expect(screen.getByText('Cuisines')).toBeDefined()
    expect(screen.getByText('Diets')).toBeDefined()
    expect(screen.getByText('Dish Types')).toBeDefined()
    expect(screen.getByText('Max Ready Time')).toBeDefined()
  })

  it('renders cuisine options from hook', () => {
    render(<FilterPanel onClose={() => {}} />)
    expect(screen.getByLabelText('Italian')).toBeDefined()
    expect(screen.getByLabelText('Mexican')).toBeDefined()
  })

  it('calls onClose when close button is clicked', () => {
    const handleClose = vi.fn()
    render(<FilterPanel onClose={handleClose} />)
    fireEvent.click(screen.getByLabelText('Close filter panel'))
    expect(handleClose).toHaveBeenCalledTimes(1)
  })

  it('toggles a cuisine filter when chip is clicked', () => {
    render(<FilterPanel onClose={() => {}} />)
    fireEvent.click(screen.getByLabelText('Italian'))
    expect(useSearchStore.getState().filters.cuisines).toContain('Italian')
  })

  it('toggles a diet filter when chip is clicked', () => {
    render(<FilterPanel onClose={() => {}} />)
    fireEvent.click(screen.getByLabelText('vegan'))
    expect(useSearchStore.getState().filters.diets).toContain('vegan')
  })

  it('sets max ready time when time chip is clicked', () => {
    render(<FilterPanel onClose={() => {}} />)
    fireEvent.click(screen.getByLabelText('30 min'))
    expect(useSearchStore.getState().filters.maxReadyTime).toBe(30)
  })

  it('clears max ready time when active time chip is clicked again', () => {
    useSearchStore.setState({ filters: { ...emptyFilters, maxReadyTime: 30 } })
    render(<FilterPanel onClose={() => {}} />)
    fireEvent.click(screen.getByLabelText('30 min'))
    expect(useSearchStore.getState().filters.maxReadyTime).toBeNull()
  })

  it('shows active filter count in header', () => {
    useSearchStore.setState({ filters: { ...emptyFilters, cuisines: ['Italian', 'Mexican'] } })
    render(<FilterPanel onClose={() => {}} />)
    expect(screen.getByText('2')).toBeDefined()
  })

  it('"Clear all filters" button clears all filters', () => {
    useSearchStore.setState({ filters: { cuisines: ['Italian'], diets: ['vegan'], dishTypes: [], maxReadyTime: 30 } })
    render(<FilterPanel onClose={() => {}} />)
    fireEvent.click(screen.getByLabelText('Clear all filters'))
    expect(useSearchStore.getState().filters).toEqual(emptyFilters)
  })

  it('"Clear all filters" button is disabled when no filters are active', () => {
    render(<FilterPanel onClose={() => {}} />)
    expect(screen.getByLabelText('Clear all filters')).toBeDisabled()
  })
})

describe('FilterButton', () => {
  it('renders without active count badge when no filters', () => {
    render(<FilterButton filters={emptyFilters} onClick={() => {}} />)
    // Badge should not exist
    expect(screen.queryByText(/^\d+$/)).toBeNull()
  })

  it('renders active count badge when filters are active', () => {
    const filters: SearchFilters = { ...emptyFilters, cuisines: ['Italian'], diets: ['vegan'] }
    render(<FilterButton filters={filters} onClick={() => {}} />)
    expect(screen.getByText('2')).toBeDefined()
  })

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn()
    render(<FilterButton filters={emptyFilters} onClick={handleClick} />)
    fireEvent.click(screen.getByRole('button'))
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('has accessible label with active count', () => {
    const filters: SearchFilters = { ...emptyFilters, cuisines: ['Italian'] }
    render(<FilterButton filters={filters} onClick={() => {}} />)
    expect(screen.getByLabelText('Filter recipes, 1 active')).toBeDefined()
  })
})
