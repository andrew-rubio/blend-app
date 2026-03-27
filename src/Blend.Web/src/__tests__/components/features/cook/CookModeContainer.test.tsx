import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { CookModeContainer } from '@/components/features/cook/CookModeContainer'
import type { CookingSession } from '@/types'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('@/stores/cookModeStore', () => ({
  useCookModeStore: vi.fn(),
}))

vi.mock('@/hooks/useCookMode', () => ({
  useSession: vi.fn(),
  useAddIngredient: vi.fn(),
  useRemoveIngredient: vi.fn(),
  useAddDish: vi.fn(),
  useRemoveDish: vi.fn(),
  useUpdateDish: vi.fn(),
  usePauseSession: vi.fn(),
  useCompleteSession: vi.fn(),
  useSuggestions: vi.fn(),
  useIngredientSearch: vi.fn(),
  useIngredientDetail: vi.fn(),
}))

import { useSession, usePauseSession, useCompleteSession, useAddIngredient, useRemoveIngredient, useAddDish, useRemoveDish, useUpdateDish, useSuggestions, useIngredientSearch, useIngredientDetail } from '@/hooks/useCookMode'
import { useCookModeStore } from '@/stores/cookModeStore'

const mockUseSession = vi.mocked(useSession)
const mockUseCookModeStore = vi.mocked(useCookModeStore)
const mockUsePauseSession = vi.mocked(usePauseSession)
const mockUseCompleteSession = vi.mocked(useCompleteSession)

const mockMutation = { mutate: vi.fn(), isPending: false }

const mockSession: CookingSession = {
  id: 'session-1',
  userId: 'user-1',
  dishes: [
    {
      dishId: 'dish-1',
      name: 'Pasta',
      ingredients: [
        { ingredientId: 'ing-1', name: 'Garlic', addedAt: '2024-01-01T00:00:00Z' },
      ],
    },
  ],
  addedIngredients: [],
  status: 'Active',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

const mockStoreState = {
  activeDishId: 'dish-1',
  selectedIngredientId: null,
  isDetailModalOpen: false,
  isSuggestionsPanelOpen: false,
  setActiveDishId: vi.fn(),
  openIngredientDetail: vi.fn(),
  closeIngredientDetail: vi.fn(),
  openSuggestionsPanel: vi.fn(),
  closeSuggestionsPanel: vi.fn(),
  reset: vi.fn(),
}

describe('CookModeContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseCookModeStore.mockReturnValue(mockStoreState)
    mockUsePauseSession.mockReturnValue(mockMutation as ReturnType<typeof usePauseSession>)
    mockUseCompleteSession.mockReturnValue(mockMutation as ReturnType<typeof useCompleteSession>)
    vi.mocked(useAddIngredient).mockReturnValue(mockMutation as ReturnType<typeof useAddIngredient>)
    vi.mocked(useRemoveIngredient).mockReturnValue(mockMutation as ReturnType<typeof useRemoveIngredient>)
    vi.mocked(useAddDish).mockReturnValue(mockMutation as ReturnType<typeof useAddDish>)
    vi.mocked(useRemoveDish).mockReturnValue(mockMutation as ReturnType<typeof useRemoveDish>)
    vi.mocked(useUpdateDish).mockReturnValue(mockMutation as ReturnType<typeof useUpdateDish>)
    vi.mocked(useSuggestions).mockReturnValue({ data: { suggestions: [], kbUnavailable: false }, isLoading: false } as ReturnType<typeof useSuggestions>)
    vi.mocked(useIngredientSearch).mockReturnValue({ data: [], isLoading: false } as ReturnType<typeof useIngredientSearch>)
    vi.mocked(useIngredientDetail).mockReturnValue({ data: undefined, isLoading: false, error: null } as ReturnType<typeof useIngredientDetail>)
  })

  it('renders loading state', () => {
    mockUseSession.mockReturnValue({ data: undefined, isLoading: true, error: null } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByTestId('cook-mode-loading')).toBeDefined()
  })

  it('renders error state', () => {
    mockUseSession.mockReturnValue({ data: undefined, isLoading: false, error: { message: 'Failed' } } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByTestId('cook-mode-error')).toBeDefined()
    expect(screen.getByText('Failed')).toBeDefined()
  })

  it('renders cook mode container with session data', () => {
    mockUseSession.mockReturnValue({ data: mockSession, isLoading: false, error: null } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByTestId('cook-mode-container')).toBeDefined()
    expect(screen.getByText('Cook Mode')).toBeDefined()
  })

  it('renders dish tabs when session has dishes', () => {
    mockUseSession.mockReturnValue({ data: mockSession, isLoading: false, error: null } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByTestId('dish-tabs')).toBeDefined()
    expect(screen.getByText('Pasta')).toBeDefined()
  })

  it('renders ingredient in workspace', () => {
    mockUseSession.mockReturnValue({ data: mockSession, isLoading: false, error: null } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByText('Garlic')).toBeDefined()
  })

  it('renders session controls', () => {
    mockUseSession.mockReturnValue({ data: mockSession, isLoading: false, error: null } as ReturnType<typeof useSession>)
    render(<CookModeContainer sessionId="session-1" />)
    expect(screen.getByTestId('session-controls')).toBeDefined()
  })
})
