import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'

// ── mocks ─────────────────────────────────────────────────────────────────────

const mockSavePreferences = vi.fn()
const mockIsPending = { value: false }
const mockIsSuccess = { value: false }
const mockSaveError = { value: null as unknown }

const savedPrefs = {
  favoriteCuisines: ['Italian', 'Japanese'],
  favoriteDishTypes: ['main course'],
  diets: ['vegetarian'],
  intolerances: ['dairy'],
  dislikedIngredientIds: ['cilantro'],
}

vi.mock('@/hooks/usePreferences', () => ({
  useUserPreferences: () => ({ data: savedPrefs, isLoading: false, error: null }),
  useSavePreferences: () => ({
    mutate: mockSavePreferences,
    isPending: mockIsPending.value,
    error: mockSaveError.value,
    isSuccess: mockIsSuccess.value,
  }),
  useCuisines: () => ({ data: ['Italian', 'Japanese', 'Mexican'], isLoading: false }),
  useDishTypes: () => ({ data: ['main course', 'dessert'], isLoading: false }),
  useDiets: () => ({ data: ['vegan', 'vegetarian'], isLoading: false }),
  useIntolerances: () => ({ data: ['dairy', 'gluten'], isLoading: false }),
}))

import { ManagePreferences } from '@/components/features/preferences/ManagePreferences'

// ── tests ──────────────────────────────────────────────────────────────────────

describe('ManagePreferences', () => {
  beforeEach(() => {
    mockSavePreferences.mockClear()
    mockIsPending.value = false
    mockIsSuccess.value = false
    mockSaveError.value = null
  })

  it('renders all preference sections', () => {
    render(<ManagePreferences />)
    expect(screen.getByText('Favorite Cuisines')).toBeDefined()
    expect(screen.getByText('Dish Types')).toBeDefined()
    expect(screen.getByText('Dietary Preferences')).toBeDefined()
    expect(screen.getByText('Intolerances')).toBeDefined()
    expect(screen.getByText('Disliked Ingredients')).toBeDefined()
  })

  it('pre-populates cuisines from saved preferences', () => {
    render(<ManagePreferences />)
    const italianChip = screen.getByRole('checkbox', { name: 'Italian' })
    expect(italianChip.getAttribute('aria-checked')).toBe('true')
  })

  it('pre-populates dish types from saved preferences', () => {
    render(<ManagePreferences />)
    const mainCourseChip = screen.getByRole('checkbox', { name: 'main course' })
    expect(mainCourseChip.getAttribute('aria-checked')).toBe('true')
  })

  it('pre-populates intolerances from saved preferences', () => {
    render(<ManagePreferences />)
    const dairyChip = screen.getByRole('checkbox', { name: 'dairy' })
    expect(dairyChip.getAttribute('aria-checked')).toBe('true')
  })

  it('pre-populates disliked ingredients from saved preferences', () => {
    render(<ManagePreferences />)
    expect(screen.getByText('cilantro')).toBeDefined()
  })

  it('toggling a cuisine updates its selection state', () => {
    render(<ManagePreferences />)
    const mexicanChip = screen.getByRole('checkbox', { name: 'Mexican' })
    expect(mexicanChip.getAttribute('aria-checked')).toBe('false')
    fireEvent.click(mexicanChip)
    expect(screen.getByRole('checkbox', { name: 'Mexican' }).getAttribute('aria-checked')).toBe(
      'true'
    )
  })

  it('calls savePreferences when Save Preferences button is clicked', async () => {
    render(<ManagePreferences />)
    fireEvent.click(screen.getByLabelText('Save preferences'))
    await waitFor(() => {
      expect(mockSavePreferences).toHaveBeenCalledWith(
        expect.objectContaining({
          favoriteCuisines: expect.arrayContaining(['Italian', 'Japanese']),
          diets: expect.arrayContaining(['vegetarian']),
        })
      )
    })
  })

  it('shows success message after saving', () => {
    mockIsSuccess.value = true
    render(<ManagePreferences />)
    expect(screen.getByRole('status')).toBeDefined()
    expect(screen.getByText('Preferences saved successfully!')).toBeDefined()
  })

  it('shows error message when save fails', () => {
    mockSaveError.value = { message: 'Save failed' }
    render(<ManagePreferences />)
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Save failed')).toBeDefined()
  })

  it('renders Clear All button', () => {
    render(<ManagePreferences />)
    expect(screen.getByLabelText('Clear all preferences')).toBeDefined()
  })

  it('shows confirmation dialog when Clear All is clicked', async () => {
    render(<ManagePreferences />)
    fireEvent.click(screen.getByLabelText('Clear all preferences'))
    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeDefined()
      expect(screen.getByText('Clear All Preferences?')).toBeDefined()
    })
  })

  it('hides dialog when Cancel is clicked', async () => {
    render(<ManagePreferences />)
    fireEvent.click(screen.getByLabelText('Clear all preferences'))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeDefined())
    fireEvent.click(screen.getByText('Cancel'))
    await waitFor(() => {
      expect(screen.queryByRole('dialog')).toBeNull()
    })
  })

  it('calls savePreferences with empty arrays when Clear All is confirmed', async () => {
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
    render(<ManagePreferences />)
    fireEvent.click(screen.getByLabelText('Clear all preferences'))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeDefined())
    fireEvent.click(screen.getByText('Yes, Clear All'))
    await waitFor(() => {
      expect(mockSavePreferences).toHaveBeenCalledWith(
        {
          favoriteCuisines: [],
          favoriteDishTypes: [],
          diets: [],
          intolerances: [],
          dislikedIngredientIds: [],
        },
        expect.any(Object)
      )
    })
  })

  it('warns about strict exclusion for intolerances', () => {
    render(<ManagePreferences />)
    expect(screen.getByText(/strictly exclude/i)).toBeDefined()
  })
})
