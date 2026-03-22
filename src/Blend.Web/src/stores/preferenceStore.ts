import { create } from 'zustand'
import type { UserPreferences } from '@/types'

export type WizardStep = 'cuisines' | 'dishTypes' | 'diets' | 'intolerances' | 'ingredients'

export const WIZARD_STEPS: WizardStep[] = [
  'cuisines',
  'dishTypes',
  'diets',
  'intolerances',
  'ingredients',
]

export const WIZARD_STEP_LABELS: Record<WizardStep, string> = {
  cuisines: 'Favorite Cuisines',
  dishTypes: 'Dish Types',
  diets: 'Dietary Preferences',
  intolerances: 'Intolerances',
  ingredients: 'Disliked Ingredients',
}

interface PreferenceSelections {
  favoriteCuisines: string[]
  favoriteDishTypes: string[]
  diets: string[]
  intolerances: string[]
  dislikedIngredientIds: string[]
}

interface PreferenceState {
  /** Current wizard step index (0-based). */
  currentStepIndex: number
  /** Whether the wizard has been completed. */
  wizardComplete: boolean
  /** Current selections during the wizard flow. */
  selections: PreferenceSelections
  /** The last saved preferences (from API). */
  savedPreferences: UserPreferences | null
}

interface PreferenceActions {
  nextStep: () => void
  prevStep: () => void
  goToStep: (index: number) => void
  setWizardComplete: (complete: boolean) => void
  toggleCuisine: (cuisine: string) => void
  toggleDishType: (dishType: string) => void
  toggleDiet: (diet: string) => void
  toggleIntolerance: (intolerance: string) => void
  addDislikedIngredient: (id: string) => void
  removeDislikedIngredient: (id: string) => void
  setSavedPreferences: (prefs: UserPreferences) => void
  populateFromSaved: (prefs: UserPreferences) => void
  resetWizard: () => void
}

type PreferenceStore = PreferenceState & PreferenceActions

const initialSelections: PreferenceSelections = {
  favoriteCuisines: [],
  favoriteDishTypes: [],
  diets: [],
  intolerances: [],
  dislikedIngredientIds: [],
}

const initialState: PreferenceState = {
  currentStepIndex: 0,
  wizardComplete: false,
  selections: initialSelections,
  savedPreferences: null,
}

export const usePreferenceStore = create<PreferenceStore>()((set, get) => ({
  ...initialState,

  nextStep: () =>
    set((state) => ({
      currentStepIndex: Math.min(state.currentStepIndex + 1, WIZARD_STEPS.length - 1),
    })),

  prevStep: () =>
    set((state) => ({
      currentStepIndex: Math.max(state.currentStepIndex - 1, 0),
    })),

  goToStep: (index: number) =>
    set({ currentStepIndex: Math.max(0, Math.min(index, WIZARD_STEPS.length - 1)) }),

  setWizardComplete: (wizardComplete: boolean) => set({ wizardComplete }),

  toggleCuisine: (cuisine: string) =>
    set((state) => {
      const current = state.selections.favoriteCuisines
      const next = current.includes(cuisine)
        ? current.filter((c) => c !== cuisine)
        : [...current, cuisine]
      return { selections: { ...state.selections, favoriteCuisines: next } }
    }),

  toggleDishType: (dishType: string) =>
    set((state) => {
      const current = state.selections.favoriteDishTypes
      const next = current.includes(dishType)
        ? current.filter((d) => d !== dishType)
        : [...current, dishType]
      return { selections: { ...state.selections, favoriteDishTypes: next } }
    }),

  toggleDiet: (diet: string) =>
    set((state) => {
      const current = state.selections.diets
      const next = current.includes(diet)
        ? current.filter((d) => d !== diet)
        : [...current, diet]
      return { selections: { ...state.selections, diets: next } }
    }),

  toggleIntolerance: (intolerance: string) =>
    set((state) => {
      const current = state.selections.intolerances
      const next = current.includes(intolerance)
        ? current.filter((i) => i !== intolerance)
        : [...current, intolerance]
      return { selections: { ...state.selections, intolerances: next } }
    }),

  addDislikedIngredient: (id: string) =>
    set((state) => {
      if (state.selections.dislikedIngredientIds.includes(id)) return state
      return {
        selections: {
          ...state.selections,
          dislikedIngredientIds: [...state.selections.dislikedIngredientIds, id],
        },
      }
    }),

  removeDislikedIngredient: (id: string) =>
    set((state) => ({
      selections: {
        ...state.selections,
        dislikedIngredientIds: state.selections.dislikedIngredientIds.filter((i) => i !== id),
      },
    })),

  setSavedPreferences: (prefs: UserPreferences) =>
    set({ savedPreferences: prefs }),

  populateFromSaved: (prefs: UserPreferences) =>
    set({
      selections: {
        favoriteCuisines: [...prefs.favoriteCuisines],
        favoriteDishTypes: [...prefs.favoriteDishTypes],
        diets: [...prefs.diets],
        intolerances: [...prefs.intolerances],
        dislikedIngredientIds: [...prefs.dislikedIngredientIds],
      },
      savedPreferences: prefs,
    }),

  resetWizard: () =>
    set({
      currentStepIndex: 0,
      wizardComplete: false,
      selections: {
        ...initialSelections,
        ...(get().savedPreferences
          ? {
              favoriteCuisines: [...(get().savedPreferences?.favoriteCuisines ?? [])],
              favoriteDishTypes: [...(get().savedPreferences?.favoriteDishTypes ?? [])],
              diets: [...(get().savedPreferences?.diets ?? [])],
              intolerances: [...(get().savedPreferences?.intolerances ?? [])],
              dislikedIngredientIds: [...(get().savedPreferences?.dislikedIngredientIds ?? [])],
            }
          : {}),
      },
    }),
}))
