import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import type { UnitSystem } from '@/types'

vi.mock('@/stores/settingsStore', () => ({
  useSettingsStore: vi.fn(),
}))

vi.mock('@/hooks/useSettings', () => ({
  useUpdateSettings: vi.fn(),
}))

import { UnitToggle } from '@/components/features/settings/UnitToggle'
import { useSettingsStore } from '@/stores/settingsStore'
import { useUpdateSettings } from '@/hooks/useSettings'

const mockUseSettingsStore = vi.mocked(useSettingsStore)
const mockUseUpdateSettings = vi.mocked(useUpdateSettings)

type MockSettingsState = {
  unitSystem: UnitSystem
  pendingDeletionDate: string | null
  setUnitSystem: () => void
  setPendingDeletionDate: () => void
}

function mockStoreWith(unitSystem: UnitSystem) {
  mockUseSettingsStore.mockImplementation(
    (selector: (s: MockSettingsState) => unknown) =>
      selector({ unitSystem, pendingDeletionDate: null, setUnitSystem: vi.fn(), setPendingDeletionDate: vi.fn() })
  )
}

describe('UnitToggle', () => {
  const mockUpdateSettings = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseUpdateSettings.mockReturnValue({ mutate: mockUpdateSettings, isPending: false } as unknown as ReturnType<typeof useUpdateSettings>)
  })

  it('renders Metric and Imperial buttons', () => {
    mockStoreWith('Metric')
    render(<UnitToggle />)
    expect(screen.getByRole('button', { name: 'Metric' })).toBeDefined()
    expect(screen.getByRole('button', { name: 'Imperial' })).toBeDefined()
  })

  it('marks Metric as pressed when unitSystem is Metric', () => {
    mockStoreWith('Metric')
    render(<UnitToggle />)
    expect(screen.getByRole('button', { name: 'Metric' }).getAttribute('aria-pressed')).toBe('true')
    expect(screen.getByRole('button', { name: 'Imperial' }).getAttribute('aria-pressed')).toBe('false')
  })

  it('marks Imperial as pressed when unitSystem is Imperial', () => {
    mockStoreWith('Imperial')
    render(<UnitToggle />)
    expect(screen.getByRole('button', { name: 'Imperial' }).getAttribute('aria-pressed')).toBe('true')
    expect(screen.getByRole('button', { name: 'Metric' }).getAttribute('aria-pressed')).toBe('false')
  })

  it('calls updateSettings when switching to Imperial', () => {
    mockStoreWith('Metric')
    render(<UnitToggle />)
    fireEvent.click(screen.getByRole('button', { name: 'Imperial' }))
    expect(mockUpdateSettings).toHaveBeenCalledWith({ unitSystem: 'Imperial' })
  })

  it('does not call updateSettings when clicking the already-selected option', () => {
    mockStoreWith('Metric')
    render(<UnitToggle />)
    fireEvent.click(screen.getByRole('button', { name: 'Metric' }))
    expect(mockUpdateSettings).not.toHaveBeenCalled()
  })

  it('disables buttons while pending', () => {
    mockStoreWith('Metric')
    mockUseUpdateSettings.mockReturnValue({ mutate: mockUpdateSettings, isPending: true } as unknown as ReturnType<typeof useUpdateSettings>)
    render(<UnitToggle />)
    expect((screen.getByRole('button', { name: 'Metric' }) as HTMLButtonElement).disabled).toBe(true)
    expect((screen.getByRole('button', { name: 'Imperial' }) as HTMLButtonElement).disabled).toBe(true)
  })
})
