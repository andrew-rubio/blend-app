import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import type { UnitSystem } from '@/types'

vi.mock('@/stores/settingsStore', () => ({
  useSettingsStore: vi.fn(),
}))

vi.mock('@/hooks/useSettings', () => ({
  useCancelAccountDeletion: vi.fn(),
}))

import { DeletionCancellationBanner } from '@/components/features/settings/DeletionCancellationBanner'
import { useSettingsStore } from '@/stores/settingsStore'
import { useCancelAccountDeletion } from '@/hooks/useSettings'

const mockUseSettingsStore = vi.mocked(useSettingsStore)
const mockUseCancelAccountDeletion = vi.mocked(useCancelAccountDeletion)

type MockSettingsState = {
  unitSystem: UnitSystem
  pendingDeletionDate: string | null
  setUnitSystem: () => void
  setPendingDeletionDate: () => void
}

function mockStoreWith(pendingDeletionDate: string | null) {
  mockUseSettingsStore.mockImplementation(
    (selector: (s: MockSettingsState) => unknown) =>
      selector({ unitSystem: 'Metric', pendingDeletionDate, setUnitSystem: vi.fn(), setPendingDeletionDate: vi.fn() })
  )
}

describe('DeletionCancellationBanner', () => {
  const mockCancelMutate = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseCancelAccountDeletion.mockReturnValue({
      mutate: mockCancelMutate,
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useCancelAccountDeletion>)
  })

  it('renders nothing when pendingDeletionDate is null', () => {
    mockStoreWith(null)
    const { container } = render(<DeletionCancellationBanner />)
    expect(container.firstChild).toBeNull()
  })

  it('renders the banner when pendingDeletionDate is set', () => {
    mockStoreWith('2026-04-25T00:00:00Z')
    render(<DeletionCancellationBanner />)
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Account deletion scheduled')).toBeDefined()
  })

  it('calls cancelDeletion when Cancel deletion is clicked', () => {
    mockStoreWith('2026-04-25T00:00:00Z')
    render(<DeletionCancellationBanner />)
    fireEvent.click(screen.getByRole('button', { name: 'Cancel deletion' }))
    expect(mockCancelMutate).toHaveBeenCalled()
  })

  it('disables the cancel button while cancellation is pending', () => {
    mockStoreWith('2026-04-25T00:00:00Z')
    mockUseCancelAccountDeletion.mockReturnValue({
      mutate: mockCancelMutate,
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useCancelAccountDeletion>)
    render(<DeletionCancellationBanner />)
    expect((screen.getByRole('button', { name: 'Cancelling…' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('shows error message when cancellation fails', () => {
    mockStoreWith('2026-04-25T00:00:00Z')
    mockUseCancelAccountDeletion.mockReturnValue({
      mutate: mockCancelMutate,
      isPending: false,
      error: { message: 'Cancellation failed', status: 500 },
    } as unknown as ReturnType<typeof useCancelAccountDeletion>)
    render(<DeletionCancellationBanner />)
    expect(screen.getByText('Cancellation failed')).toBeDefined()
  })
})
