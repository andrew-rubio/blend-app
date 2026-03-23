import { create } from 'zustand'

interface CookModeState {
  activeDishId: string | null
  selectedIngredientId: string | null
  isSuggestionsPanelOpen: boolean
  isDetailModalOpen: boolean
}

interface CookModeActions {
  setActiveDishId: (id: string | null) => void
  openIngredientDetail: (ingredientId: string) => void
  closeIngredientDetail: () => void
  openSuggestionsPanel: () => void
  closeSuggestionsPanel: () => void
  reset: () => void
}

export type CookModeStore = CookModeState & CookModeActions

const initialState: CookModeState = {
  activeDishId: null,
  selectedIngredientId: null,
  isSuggestionsPanelOpen: false,
  isDetailModalOpen: false,
}

export const useCookModeStore = create<CookModeStore>()((set) => ({
  ...initialState,

  setActiveDishId: (id) => set({ activeDishId: id }),

  openIngredientDetail: (ingredientId) =>
    set({ selectedIngredientId: ingredientId, isDetailModalOpen: true }),

  closeIngredientDetail: () =>
    set({ selectedIngredientId: null, isDetailModalOpen: false }),

  openSuggestionsPanel: () => set({ isSuggestionsPanelOpen: true }),

  closeSuggestionsPanel: () => set({ isSuggestionsPanelOpen: false }),

  reset: () => set(initialState),
}))
