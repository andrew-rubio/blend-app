import { describe, it, expect, beforeEach } from 'vitest'
import { usePreferenceStore } from '@/stores/preferenceStore'

const mockPreferences = {
  favoriteCuisines: ['Italian', 'Japanese'],
  favoriteDishTypes: ['main course'],
  diets: ['vegetarian'],
  intolerances: ['dairy'],
  dislikedIngredientIds: ['cilantro'],
}

describe('preferenceStore', () => {
  beforeEach(() => {
    usePreferenceStore.setState({
      currentStepIndex: 0,
      wizardComplete: false,
      selections: {
        favoriteCuisines: [],
        favoriteDishTypes: [],
        diets: [],
        intolerances: [],
        dislikedIngredientIds: [],
      },
      savedPreferences: null,
    })
  })

  it('should have correct initial state', () => {
    const state = usePreferenceStore.getState()
    expect(state.currentStepIndex).toBe(0)
    expect(state.wizardComplete).toBe(false)
    expect(state.selections.favoriteCuisines).toEqual([])
    expect(state.savedPreferences).toBeNull()
  })

  describe('wizard navigation', () => {
    it('nextStep increments the step index', () => {
      const { nextStep } = usePreferenceStore.getState()
      nextStep()
      expect(usePreferenceStore.getState().currentStepIndex).toBe(1)
    })

    it('prevStep decrements the step index', () => {
      usePreferenceStore.setState({ currentStepIndex: 2 })
      const { prevStep } = usePreferenceStore.getState()
      prevStep()
      expect(usePreferenceStore.getState().currentStepIndex).toBe(1)
    })

    it('nextStep does not exceed last step', () => {
      usePreferenceStore.setState({ currentStepIndex: 4 })
      const { nextStep } = usePreferenceStore.getState()
      nextStep()
      expect(usePreferenceStore.getState().currentStepIndex).toBe(4)
    })

    it('prevStep does not go below 0', () => {
      usePreferenceStore.setState({ currentStepIndex: 0 })
      const { prevStep } = usePreferenceStore.getState()
      prevStep()
      expect(usePreferenceStore.getState().currentStepIndex).toBe(0)
    })

    it('goToStep navigates to the given index', () => {
      const { goToStep } = usePreferenceStore.getState()
      goToStep(3)
      expect(usePreferenceStore.getState().currentStepIndex).toBe(3)
    })

    it('goToStep clamps to valid range', () => {
      const { goToStep } = usePreferenceStore.getState()
      goToStep(-5)
      expect(usePreferenceStore.getState().currentStepIndex).toBe(0)
      goToStep(100)
      expect(usePreferenceStore.getState().currentStepIndex).toBe(4)
    })

    it('setWizardComplete updates wizardComplete flag', () => {
      const { setWizardComplete } = usePreferenceStore.getState()
      setWizardComplete(true)
      expect(usePreferenceStore.getState().wizardComplete).toBe(true)
    })
  })

  describe('cuisine toggling', () => {
    it('toggleCuisine adds a cuisine when not selected', () => {
      const { toggleCuisine } = usePreferenceStore.getState()
      toggleCuisine('Italian')
      expect(usePreferenceStore.getState().selections.favoriteCuisines).toContain('Italian')
    })

    it('toggleCuisine removes a cuisine when already selected', () => {
      usePreferenceStore.setState({
        selections: { ...usePreferenceStore.getState().selections, favoriteCuisines: ['Italian'] },
      })
      const { toggleCuisine } = usePreferenceStore.getState()
      toggleCuisine('Italian')
      expect(usePreferenceStore.getState().selections.favoriteCuisines).not.toContain('Italian')
    })

    it('toggleCuisine supports multiple selections', () => {
      const { toggleCuisine } = usePreferenceStore.getState()
      toggleCuisine('Italian')
      toggleCuisine('Japanese')
      expect(usePreferenceStore.getState().selections.favoriteCuisines).toEqual([
        'Italian',
        'Japanese',
      ])
    })
  })

  describe('dish type toggling', () => {
    it('toggleDishType adds and removes dish types', () => {
      const { toggleDishType } = usePreferenceStore.getState()
      toggleDishType('dessert')
      expect(usePreferenceStore.getState().selections.favoriteDishTypes).toContain('dessert')
      toggleDishType('dessert')
      expect(usePreferenceStore.getState().selections.favoriteDishTypes).not.toContain('dessert')
    })
  })

  describe('diet toggling', () => {
    it('toggleDiet adds and removes diets', () => {
      const { toggleDiet } = usePreferenceStore.getState()
      toggleDiet('vegan')
      expect(usePreferenceStore.getState().selections.diets).toContain('vegan')
      toggleDiet('vegan')
      expect(usePreferenceStore.getState().selections.diets).not.toContain('vegan')
    })
  })

  describe('intolerance toggling', () => {
    it('toggleIntolerance adds and removes intolerances', () => {
      const { toggleIntolerance } = usePreferenceStore.getState()
      toggleIntolerance('dairy')
      expect(usePreferenceStore.getState().selections.intolerances).toContain('dairy')
      toggleIntolerance('dairy')
      expect(usePreferenceStore.getState().selections.intolerances).not.toContain('dairy')
    })
  })

  describe('disliked ingredients', () => {
    it('addDislikedIngredient adds an ingredient id', () => {
      const { addDislikedIngredient } = usePreferenceStore.getState()
      addDislikedIngredient('cilantro')
      expect(usePreferenceStore.getState().selections.dislikedIngredientIds).toContain('cilantro')
    })

    it('addDislikedIngredient does not add duplicates', () => {
      const { addDislikedIngredient } = usePreferenceStore.getState()
      addDislikedIngredient('cilantro')
      addDislikedIngredient('cilantro')
      expect(
        usePreferenceStore.getState().selections.dislikedIngredientIds.filter((i) => i === 'cilantro')
      ).toHaveLength(1)
    })

    it('removeDislikedIngredient removes an ingredient id', () => {
      usePreferenceStore.setState({
        selections: {
          ...usePreferenceStore.getState().selections,
          dislikedIngredientIds: ['cilantro', 'mushroom'],
        },
      })
      const { removeDislikedIngredient } = usePreferenceStore.getState()
      removeDislikedIngredient('cilantro')
      expect(usePreferenceStore.getState().selections.dislikedIngredientIds).not.toContain(
        'cilantro'
      )
      expect(usePreferenceStore.getState().selections.dislikedIngredientIds).toContain('mushroom')
    })
  })

  describe('saved preferences', () => {
    it('setSavedPreferences stores preferences', () => {
      const { setSavedPreferences } = usePreferenceStore.getState()
      setSavedPreferences(mockPreferences)
      expect(usePreferenceStore.getState().savedPreferences).toEqual(mockPreferences)
    })

    it('populateFromSaved pre-fills selections from saved preferences', () => {
      const { populateFromSaved } = usePreferenceStore.getState()
      populateFromSaved(mockPreferences)
      const state = usePreferenceStore.getState()
      expect(state.selections.favoriteCuisines).toEqual(['Italian', 'Japanese'])
      expect(state.selections.favoriteDishTypes).toEqual(['main course'])
      expect(state.selections.diets).toEqual(['vegetarian'])
      expect(state.selections.intolerances).toEqual(['dairy'])
      expect(state.selections.dislikedIngredientIds).toEqual(['cilantro'])
    })

    it('resetWizard resets step to 0 and restores selections from savedPreferences', () => {
      usePreferenceStore.setState({
        currentStepIndex: 3,
        savedPreferences: mockPreferences,
        selections: {
          favoriteCuisines: ['Greek'],
          favoriteDishTypes: [],
          diets: [],
          intolerances: [],
          dislikedIngredientIds: [],
        },
      })
      const { resetWizard } = usePreferenceStore.getState()
      resetWizard()
      const state = usePreferenceStore.getState()
      expect(state.currentStepIndex).toBe(0)
      expect(state.selections.favoriteCuisines).toEqual(['Italian', 'Japanese'])
    })
  })
})
