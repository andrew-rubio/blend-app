import { describe, it, expect, beforeEach } from 'vitest'
import { useCookModeStore } from '@/stores/cookModeStore'

describe('cookModeStore', () => {
  beforeEach(() => {
    useCookModeStore.setState({
      activeDishId: null,
      selectedIngredientId: null,
      isSuggestionsPanelOpen: false,
      isDetailModalOpen: false,
    })
  })

  it('has correct initial state', () => {
    const state = useCookModeStore.getState()
    expect(state.activeDishId).toBeNull()
    expect(state.selectedIngredientId).toBeNull()
    expect(state.isSuggestionsPanelOpen).toBe(false)
    expect(state.isDetailModalOpen).toBe(false)
  })

  it('setActiveDishId updates activeDishId', () => {
    useCookModeStore.getState().setActiveDishId('dish-1')
    expect(useCookModeStore.getState().activeDishId).toBe('dish-1')
  })

  it('setActiveDishId accepts null', () => {
    useCookModeStore.getState().setActiveDishId('dish-1')
    useCookModeStore.getState().setActiveDishId(null)
    expect(useCookModeStore.getState().activeDishId).toBeNull()
  })

  it('openIngredientDetail sets ingredientId and opens modal', () => {
    useCookModeStore.getState().openIngredientDetail('ing-42')
    const state = useCookModeStore.getState()
    expect(state.selectedIngredientId).toBe('ing-42')
    expect(state.isDetailModalOpen).toBe(true)
  })

  it('closeIngredientDetail clears ingredientId and closes modal', () => {
    useCookModeStore.getState().openIngredientDetail('ing-42')
    useCookModeStore.getState().closeIngredientDetail()
    const state = useCookModeStore.getState()
    expect(state.selectedIngredientId).toBeNull()
    expect(state.isDetailModalOpen).toBe(false)
  })

  it('openSuggestionsPanel sets isSuggestionsPanelOpen to true', () => {
    useCookModeStore.getState().openSuggestionsPanel()
    expect(useCookModeStore.getState().isSuggestionsPanelOpen).toBe(true)
  })

  it('closeSuggestionsPanel sets isSuggestionsPanelOpen to false', () => {
    useCookModeStore.getState().openSuggestionsPanel()
    useCookModeStore.getState().closeSuggestionsPanel()
    expect(useCookModeStore.getState().isSuggestionsPanelOpen).toBe(false)
  })

  it('reset restores initial state', () => {
    useCookModeStore.getState().setActiveDishId('dish-1')
    useCookModeStore.getState().openIngredientDetail('ing-1')
    useCookModeStore.getState().openSuggestionsPanel()
    useCookModeStore.getState().reset()
    const state = useCookModeStore.getState()
    expect(state.activeDishId).toBeNull()
    expect(state.selectedIngredientId).toBeNull()
    expect(state.isSuggestionsPanelOpen).toBe(false)
    expect(state.isDetailModalOpen).toBe(false)
  })
})
