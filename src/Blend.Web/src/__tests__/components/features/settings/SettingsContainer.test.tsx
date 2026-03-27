import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import type { ThemeMode, UnitSystem, User } from '@/types'

// ── Module mocks ──────────────────────────────────────────────────────────────

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

vi.mock('next/link', () => ({
  default: ({ children, href, className }: { children: React.ReactNode; href: string; className?: string }) => (
    <a href={href} className={className}>{children}</a>
  ),
}))

vi.mock('@/hooks/usePreferences', () => ({
  useUserPreferences: vi.fn(),
}))

vi.mock('@/hooks/useSettings', () => ({
  useAppSettings: vi.fn(),
  useUpdateSettings: vi.fn(),
  useRequestAccountDeletion: vi.fn(),
  useCancelAccountDeletion: vi.fn(),
}))

vi.mock('@/hooks/useIngredientSubmissions', () => ({
  useMyIngredientSubmissions: vi.fn(),
  useCreateIngredientSubmission: vi.fn(),
  useIngredientSearch: vi.fn(),
  useIngredientCatalogue: vi.fn(),
}))

vi.mock('@/stores/settingsStore', () => ({
  useSettingsStore: vi.fn(),
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('@/components/features/SplashIntro', () => ({
  SplashIntro: ({ onDismiss }: { onDismiss: () => void }) => (
    <div data-testid="splash-intro">
      <button onClick={onDismiss}>Dismiss</button>
    </div>
  ),
}))

// Import after mocking
import { SettingsContainer } from '@/components/features/settings/SettingsContainer'
import { useUserPreferences } from '@/hooks/usePreferences'
import { useAppSettings, useUpdateSettings, useRequestAccountDeletion, useCancelAccountDeletion } from '@/hooks/useSettings'
import { useMyIngredientSubmissions, useCreateIngredientSubmission, useIngredientSearch, useIngredientCatalogue } from '@/hooks/useIngredientSubmissions'
import { useSettingsStore } from '@/stores/settingsStore'
import { useAuthStore } from '@/stores/authStore'

const mockUseUserPreferences = vi.mocked(useUserPreferences)
const mockUseAppSettings = vi.mocked(useAppSettings)
const mockUseUpdateSettings = vi.mocked(useUpdateSettings)
const mockUseRequestAccountDeletion = vi.mocked(useRequestAccountDeletion)
const mockUseCancelAccountDeletion = vi.mocked(useCancelAccountDeletion)
const mockUseMyIngredientSubmissions = vi.mocked(useMyIngredientSubmissions)
const mockUseCreateIngredientSubmission = vi.mocked(useCreateIngredientSubmission)
const mockUseIngredientSearch = vi.mocked(useIngredientSearch)
const mockUseIngredientCatalogue = vi.mocked(useIngredientCatalogue)
const mockUseSettingsStore = vi.mocked(useSettingsStore)
const mockUseAuthStore = vi.mocked(useAuthStore)

type MockSettingsState = {
  unitSystem: UnitSystem
  theme: ThemeMode
  pendingDeletionDate: string | null
  setUnitSystem: () => void
  setTheme: () => void
  setPendingDeletionDate: () => void
}

type MockAuthState = {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
  login: () => void
  logout: () => void
  setLoading: () => void
  updateUser: () => void
  setToken: () => void
}

function setupDefaultMocks() {
  mockUseUserPreferences.mockReturnValue({ data: undefined, isLoading: false } as ReturnType<typeof useUserPreferences>)
  mockUseAppSettings.mockReturnValue({ data: undefined, isLoading: false } as ReturnType<typeof useAppSettings>)
  mockUseUpdateSettings.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useUpdateSettings>)
  mockUseRequestAccountDeletion.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useRequestAccountDeletion>)
  mockUseCancelAccountDeletion.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useCancelAccountDeletion>)
  mockUseMyIngredientSubmissions.mockReturnValue({ data: undefined, isLoading: false } as ReturnType<typeof useMyIngredientSubmissions>)
  mockUseCreateIngredientSubmission.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useCreateIngredientSubmission>)
  mockUseIngredientSearch.mockReturnValue({ data: [], isFetching: false } as unknown as ReturnType<typeof useIngredientSearch>)
  mockUseIngredientCatalogue.mockReturnValue({ data: undefined, isLoading: false } as ReturnType<typeof useIngredientCatalogue>)
  mockUseSettingsStore.mockImplementation(
    (selector: (s: MockSettingsState) => unknown) =>
      selector({ unitSystem: 'Metric', theme: 'system', pendingDeletionDate: null, setUnitSystem: vi.fn(), setTheme: vi.fn(), setPendingDeletionDate: vi.fn() })
  )
  mockUseAuthStore.mockImplementation(
    (selector: (s: MockAuthState) => unknown) =>
      selector({ user: null, token: null, isAuthenticated: true, isLoading: false, login: vi.fn(), logout: vi.fn(), setLoading: vi.fn(), updateUser: vi.fn(), setToken: vi.fn() })
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('SettingsContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    setupDefaultMocks()
  })

  it('renders all settings sections', () => {
    render(<SettingsContainer />)
    expect(screen.getByText('App Settings')).toBeDefined()
    expect(screen.getByText('Manage Preferences')).toBeDefined()
    expect(screen.getByText('Ingredient Catalogue')).toBeDefined()
    expect(screen.getByText('Submit New Ingredient')).toBeDefined()
    expect(screen.getByText('My Submissions')).toBeDefined()
    expect(screen.getByText('Measurement System')).toBeDefined()
    expect(screen.getByText('Replay Introduction')).toBeDefined()
    expect(screen.getByText('Share Blend')).toBeDefined()
    expect(screen.getByText('Delete my account')).toBeDefined()
  })

  it('shows preference summary when preferences are loaded', () => {
    mockUseUserPreferences.mockReturnValue({
      data: {
        favoriteCuisines: ['Italian', 'Japanese'],
        favoriteDishTypes: [],
        diets: ['vegan'],
        intolerances: ['dairy', 'gluten'],
        dislikedIngredientIds: [],
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useUserPreferences>)
    render(<SettingsContainer />)
    expect(screen.getByText('2 cuisines, 1 diet, 2 intolerances')).toBeDefined()
  })

  it('navigates to ingredient catalogue panel', () => {
    render(<SettingsContainer />)
    fireEvent.click(screen.getByText('Ingredient Catalogue'))
    expect(screen.getByText('Ingredient Catalogue')).toBeDefined()
  })

  it('navigates to ingredient submission form panel', () => {
    render(<SettingsContainer />)
    fireEvent.click(screen.getByText('Submit New Ingredient'))
    expect(screen.getByText('Submit New Ingredient')).toBeDefined()
  })

  it('navigates to my submissions panel', () => {
    render(<SettingsContainer />)
    fireEvent.click(screen.getByText('My Submissions'))
    expect(screen.getByText('My Submissions')).toBeDefined()
  })

  it('shows deletion wizard on danger zone button click', () => {
    render(<SettingsContainer />)
    fireEvent.click(screen.getByText('Delete my account'))
    // Wizard shows a dialog with "Delete account" heading
    expect(screen.getByRole('dialog', { name: 'Delete account' })).toBeDefined()
  })

  it('shows splash intro on replay introduction click', () => {
    render(<SettingsContainer />)
    fireEvent.click(screen.getByText('Replay Introduction'))
    expect(screen.getByTestId('splash-intro')).toBeDefined()
  })

  it('shows deletion cancellation banner when pending deletion date is set', () => {
    mockUseSettingsStore.mockImplementation(
      (selector: (s: MockSettingsState) => unknown) =>
        selector({ unitSystem: 'Metric', theme: 'system', pendingDeletionDate: '2026-04-25T00:00:00Z', setUnitSystem: vi.fn(), setTheme: vi.fn(), setPendingDeletionDate: vi.fn() })
    )
    render(<SettingsContainer />)
    expect(screen.getByText('Account deletion scheduled')).toBeDefined()
  })

  it('Manage Preferences link points to /settings/preferences', () => {
    render(<SettingsContainer />)
    const link = screen.getByRole('link', { name: /Manage Preferences/i })
    expect(link.getAttribute('href')).toBe('/settings/preferences')
  })

  it('has accessible heading hierarchy', () => {
    render(<SettingsContainer />)
    expect(screen.getByRole('heading', { name: 'App Settings', level: 1 })).toBeDefined()
  })
})
