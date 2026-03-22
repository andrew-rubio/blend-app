import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'

// ── mocks ─────────────────────────────────────────────────────────────────────

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

const mockSavePreferences = vi.fn()
const mockIsPending = { value: false }
const mockError = { value: null as unknown }
vi.mock('@/hooks/usePreferences', () => ({
  useSavePreferences: () => ({
    mutate: mockSavePreferences,
    isPending: mockIsPending.value,
    error: mockError.value,
  }),
  useCuisines: () => ({ data: ['Italian', 'Japanese', 'Mexican'], isLoading: false }),
  useDishTypes: () => ({ data: ['main course', 'dessert'], isLoading: false }),
  useDiets: () => ({ data: ['vegan', 'vegetarian'], isLoading: false }),
  useIntolerances: () => ({ data: ['dairy', 'gluten'], isLoading: false }),
}))

import { PreferenceWizard } from '@/components/features/preferences/PreferenceWizard'
import { usePreferenceStore } from '@/stores/preferenceStore'

// ── helpers ────────────────────────────────────────────────────────────────────

function resetStore() {
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
}

// ── tests ──────────────────────────────────────────────────────────────────────

describe('PreferenceWizard', () => {
  beforeEach(() => {
    resetStore()
    mockPush.mockClear()
    mockSavePreferences.mockClear()
    mockIsPending.value = false
    mockError.value = null
  })

  it('renders the wizard header', () => {
    render(<PreferenceWizard />)
    expect(screen.getByText('Set Up Your Preferences')).toBeDefined()
  })

  it('shows step 1 (Cuisines) by default', () => {
    render(<PreferenceWizard />)
    expect(screen.getByText('Favorite Cuisines')).toBeDefined()
    expect(screen.getByText('Italian')).toBeDefined()
    expect(screen.getByText('Japanese')).toBeDefined()
  })

  it('progress indicator shows Step 1 of 5', () => {
    render(<PreferenceWizard />)
    expect(screen.getByText('Step 1 of 5')).toBeDefined()
  })

  it('Back button is disabled on the first step', () => {
    render(<PreferenceWizard />)
    expect(screen.getByLabelText('Go to previous step')).toBeDisabled()
  })

  it('Next button advances to step 2', async () => {
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByLabelText('Go to next step'))
    await waitFor(() => {
      expect(screen.getByText('Dish Types')).toBeDefined()
      expect(screen.getByText('Step 2 of 5')).toBeDefined()
    })
  })

  it('Back button is enabled after advancing to step 2', async () => {
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByLabelText('Go to next step'))
    await waitFor(() => {
      expect(screen.getByLabelText('Go to previous step')).not.toBeDisabled()
    })
  })

  it('Back button returns to previous step', async () => {
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByLabelText('Go to next step'))
    await waitFor(() => expect(screen.getByText('Dish Types')).toBeDefined())
    fireEvent.click(screen.getByLabelText('Go to previous step'))
    await waitFor(() => expect(screen.getByText('Favorite Cuisines')).toBeDefined())
  })

  it('Skip button advances to next step on non-last steps', async () => {
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByLabelText('Skip this step'))
    await waitFor(() => {
      expect(screen.getByText('Dish Types')).toBeDefined()
    })
  })

  it('selecting a cuisine chip updates store state', async () => {
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByRole('checkbox', { name: 'Italian' }))
    await waitFor(() => {
      expect(usePreferenceStore.getState().selections.favoriteCuisines).toContain('Italian')
    })
  })

  it('deselecting a cuisine chip updates store state', async () => {
    usePreferenceStore.setState({
      ...usePreferenceStore.getState(),
      selections: {
        ...usePreferenceStore.getState().selections,
        favoriteCuisines: ['Italian'],
      },
    })
    render(<PreferenceWizard />)
    fireEvent.click(screen.getByRole('checkbox', { name: 'Italian' }))
    await waitFor(() => {
      expect(usePreferenceStore.getState().selections.favoriteCuisines).not.toContain('Italian')
    })
  })

  it('shows "Save & Continue" button on last step', async () => {
    usePreferenceStore.setState({ ...usePreferenceStore.getState(), currentStepIndex: 4 })
    render(<PreferenceWizard />)
    expect(screen.getByLabelText('Save preferences and continue')).toBeDefined()
  })

  it('calls savePreferences with all selections on save', async () => {
    usePreferenceStore.setState({
      currentStepIndex: 4,
      wizardComplete: false,
      selections: {
        favoriteCuisines: ['Italian'],
        favoriteDishTypes: ['dessert'],
        diets: ['vegan'],
        intolerances: ['dairy'],
        dislikedIngredientIds: ['cilantro'],
      },
      savedPreferences: null,
    })

    render(<PreferenceWizard />)
    fireEvent.click(screen.getByLabelText('Save preferences and continue'))

    await waitFor(() => {
      expect(mockSavePreferences).toHaveBeenCalledWith(
        {
          favoriteCuisines: ['Italian'],
          favoriteDishTypes: ['dessert'],
          diets: ['vegan'],
          intolerances: ['dairy'],
          dislikedIngredientIds: ['cilantro'],
        },
        expect.any(Object)
      )
    })
  })

  it('shows an error message when savePreferences fails', async () => {
    mockError.value = { message: 'Server error' }
    usePreferenceStore.setState({ ...usePreferenceStore.getState(), currentStepIndex: 4 })
    render(<PreferenceWizard />)
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Server error')).toBeDefined()
  })

  it('shows loading spinner on Save & Continue button when saving', async () => {
    mockIsPending.value = true
    usePreferenceStore.setState({ ...usePreferenceStore.getState(), currentStepIndex: 4 })
    const { container } = render(<PreferenceWizard />)
    expect(container.querySelector('.animate-spin')).toBeDefined()
  })

  it('calls onComplete callback after successful save', async () => {
    const onComplete = vi.fn()
    mockSavePreferences.mockImplementationOnce(
      (_data: unknown, options: { onSuccess: (v: unknown) => void }) => {
        options.onSuccess({
          favoriteCuisines: [],
          favoriteDishTypes: [],
          diets: [],
          intolerances: [],
          dislikedIngredientIds: [],
        })
      }
    )

    usePreferenceStore.setState({ ...usePreferenceStore.getState(), currentStepIndex: 4 })
    render(<PreferenceWizard onComplete={onComplete} />)
    fireEvent.click(screen.getByLabelText('Save preferences and continue'))
    await waitFor(() => {
      expect(onComplete).toHaveBeenCalled()
    })
  })
})
