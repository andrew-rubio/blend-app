import { describe, it, expect, beforeEach } from 'vitest'
import { useSearchStore, selectActiveFilterCount } from '@/stores/searchStore'
import type { SearchFilters } from '@/types'

const emptyFilters: SearchFilters = {
  cuisines: [],
  diets: [],
  dishTypes: [],
  maxReadyTime: null,
}

describe('searchStore', () => {
  beforeEach(() => {
    useSearchStore.setState({
      query: '',
      filters: emptyFilters,
      isFilterPanelOpen: false,
    })
  })

  it('should have correct initial state', () => {
    const state = useSearchStore.getState()
    expect(state.query).toBe('')
    expect(state.filters).toEqual(emptyFilters)
    expect(state.isFilterPanelOpen).toBe(false)
  })

  describe('setQuery', () => {
    it('updates the query', () => {
      useSearchStore.getState().setQuery('pasta')
      expect(useSearchStore.getState().query).toBe('pasta')
    })
  })

  describe('setFilters', () => {
    it('merges partial filters', () => {
      useSearchStore.getState().setFilters({ cuisines: ['Italian'] })
      expect(useSearchStore.getState().filters.cuisines).toEqual(['Italian'])
      expect(useSearchStore.getState().filters.diets).toEqual([])
    })
  })

  describe('toggleCuisineFilter', () => {
    it('adds a cuisine filter', () => {
      useSearchStore.getState().toggleCuisineFilter('Italian')
      expect(useSearchStore.getState().filters.cuisines).toContain('Italian')
    })

    it('removes a cuisine filter when toggled again', () => {
      useSearchStore.setState({ filters: { ...emptyFilters, cuisines: ['Italian'] } })
      useSearchStore.getState().toggleCuisineFilter('Italian')
      expect(useSearchStore.getState().filters.cuisines).not.toContain('Italian')
    })

    it('can add multiple cuisines', () => {
      useSearchStore.getState().toggleCuisineFilter('Italian')
      useSearchStore.getState().toggleCuisineFilter('Mexican')
      expect(useSearchStore.getState().filters.cuisines).toEqual(['Italian', 'Mexican'])
    })
  })

  describe('toggleDietFilter', () => {
    it('adds a diet filter', () => {
      useSearchStore.getState().toggleDietFilter('vegan')
      expect(useSearchStore.getState().filters.diets).toContain('vegan')
    })

    it('removes a diet filter when toggled again', () => {
      useSearchStore.setState({ filters: { ...emptyFilters, diets: ['vegan'] } })
      useSearchStore.getState().toggleDietFilter('vegan')
      expect(useSearchStore.getState().filters.diets).not.toContain('vegan')
    })
  })

  describe('toggleDishTypeFilter', () => {
    it('adds a dish type filter', () => {
      useSearchStore.getState().toggleDishTypeFilter('main course')
      expect(useSearchStore.getState().filters.dishTypes).toContain('main course')
    })

    it('removes a dish type filter when toggled again', () => {
      useSearchStore.setState({ filters: { ...emptyFilters, dishTypes: ['dessert'] } })
      useSearchStore.getState().toggleDishTypeFilter('dessert')
      expect(useSearchStore.getState().filters.dishTypes).not.toContain('dessert')
    })
  })

  describe('setMaxReadyTime', () => {
    it('sets max ready time', () => {
      useSearchStore.getState().setMaxReadyTime(30)
      expect(useSearchStore.getState().filters.maxReadyTime).toBe(30)
    })

    it('clears max ready time when set to null', () => {
      useSearchStore.setState({ filters: { ...emptyFilters, maxReadyTime: 30 } })
      useSearchStore.getState().setMaxReadyTime(null)
      expect(useSearchStore.getState().filters.maxReadyTime).toBeNull()
    })
  })

  describe('clearFilters', () => {
    it('resets all filters to empty', () => {
      useSearchStore.setState({
        filters: { cuisines: ['Italian'], diets: ['vegan'], dishTypes: ['dessert'], maxReadyTime: 30 },
      })
      useSearchStore.getState().clearFilters()
      expect(useSearchStore.getState().filters).toEqual(emptyFilters)
    })
  })

  describe('filter panel', () => {
    it('openFilterPanel sets isFilterPanelOpen to true', () => {
      useSearchStore.getState().openFilterPanel()
      expect(useSearchStore.getState().isFilterPanelOpen).toBe(true)
    })

    it('closeFilterPanel sets isFilterPanelOpen to false', () => {
      useSearchStore.setState({ isFilterPanelOpen: true })
      useSearchStore.getState().closeFilterPanel()
      expect(useSearchStore.getState().isFilterPanelOpen).toBe(false)
    })
  })

  describe('reset', () => {
    it('resets all state to initial values', () => {
      useSearchStore.setState({
        query: 'pizza',
        filters: { cuisines: ['Italian'], diets: ['vegan'], dishTypes: ['main course'], maxReadyTime: 30 },
        isFilterPanelOpen: true,
      })
      useSearchStore.getState().reset()
      const state = useSearchStore.getState()
      expect(state.query).toBe('')
      expect(state.filters).toEqual(emptyFilters)
      expect(state.isFilterPanelOpen).toBe(false)
    })
  })
})

describe('selectActiveFilterCount', () => {
  it('returns 0 for empty filters', () => {
    expect(selectActiveFilterCount(emptyFilters)).toBe(0)
  })

  it('counts cuisine filters', () => {
    expect(selectActiveFilterCount({ ...emptyFilters, cuisines: ['Italian', 'Mexican'] })).toBe(2)
  })

  it('counts diet filters', () => {
    expect(selectActiveFilterCount({ ...emptyFilters, diets: ['vegan'] })).toBe(1)
  })

  it('counts dish type filters', () => {
    expect(selectActiveFilterCount({ ...emptyFilters, dishTypes: ['dessert', 'soup'] })).toBe(2)
  })

  it('counts maxReadyTime as 1 filter when set', () => {
    expect(selectActiveFilterCount({ ...emptyFilters, maxReadyTime: 30 })).toBe(1)
  })

  it('sums all active filters', () => {
    expect(
      selectActiveFilterCount({
        cuisines: ['Italian'],
        diets: ['vegan'],
        dishTypes: ['dessert'],
        maxReadyTime: 30,
      })
    ).toBe(4)
  })
})
