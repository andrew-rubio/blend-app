import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import type { User } from '@/types'

vi.mock('@/hooks/useSettings', () => ({
  useRequestAccountDeletion: vi.fn(),
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

import { AccountDeletionWizard } from '@/components/features/settings/AccountDeletionWizard'
import { useRequestAccountDeletion } from '@/hooks/useSettings'
import { useAuthStore } from '@/stores/authStore'

const mockUseRequestAccountDeletion = vi.mocked(useRequestAccountDeletion)
const mockUseAuthStore = vi.mocked(useAuthStore)

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

function mockAuthStore(logout: () => void) {
  mockUseAuthStore.mockImplementation(
    (selector: (s: MockAuthState) => unknown) =>
      selector({
        user: null,
        token: null,
        isAuthenticated: true,
        isLoading: false,
        login: vi.fn(),
        logout,
        setLoading: vi.fn(),
        updateUser: vi.fn(),
        setToken: vi.fn(),
      })
  )
}

describe('AccountDeletionWizard', () => {
  const mockOnClose = vi.fn()
  const mockMutate = vi.fn()
  const mockLogout = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseRequestAccountDeletion.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useRequestAccountDeletion>)
    mockAuthStore(mockLogout)
  })

  it('renders the warning step first', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    expect(screen.getByText('This action is permanent')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Continue' })).toBeDefined()
  })

  it('closes the dialog from the warning step', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(mockOnClose).toHaveBeenCalled()
  })

  it('advances to re-authentication step', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    expect(screen.getByLabelText('Password')).toBeDefined()
  })

  it('shows error if password is empty on reauth step', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Please enter your password.')).toBeDefined()
  })

  it('advances to confirmation step after password entry', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    expect(screen.getByText(/To confirm, type/i, { exact: false })).toBeDefined()
  })

  it('keeps delete button disabled when confirmation text is wrong', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.change(screen.getByLabelText(/Type DELETE to confirm deletion/i), { target: { value: 'delete' } })
    expect((screen.getByRole('button', { name: 'Delete my account' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('calls mutate when DELETE is typed correctly', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.change(screen.getByLabelText(/Type DELETE to confirm deletion/i), { target: { value: 'DELETE' } })
    fireEvent.click(screen.getByRole('button', { name: 'Delete my account' }))
    expect(mockMutate).toHaveBeenCalledWith({ password: 'mypassword' }, expect.any(Object))
  })

  it('shows success screen after successful deletion request', () => {
    mockUseRequestAccountDeletion.mockReturnValue({
      mutate: (_data: unknown, opts?: { onSuccess?: (result: { scheduledDeletionDate: string }) => void }) => {
        opts?.onSuccess?.({ scheduledDeletionDate: '2026-04-25T00:00:00Z' })
      },
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useRequestAccountDeletion>)
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.change(screen.getByLabelText(/Type DELETE to confirm deletion/i), { target: { value: 'DELETE' } })
    fireEvent.click(screen.getByRole('button', { name: 'Delete my account' }))
    expect(screen.getByText('permanently deleted in 30 days', { exact: false })).toBeDefined()
  })

  it('calls logout and onClose after sign out on success screen', () => {
    mockUseRequestAccountDeletion.mockReturnValue({
      mutate: (_data: unknown, opts?: { onSuccess?: (result: { scheduledDeletionDate: string }) => void }) => {
        opts?.onSuccess?.({ scheduledDeletionDate: '2026-04-25T00:00:00Z' })
      },
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useRequestAccountDeletion>)
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.change(screen.getByLabelText(/Type DELETE to confirm deletion/i), { target: { value: 'DELETE' } })
    fireEvent.click(screen.getByRole('button', { name: 'Delete my account' }))
    fireEvent.click(screen.getByRole('button', { name: 'Sign out' }))
    expect(mockLogout).toHaveBeenCalled()
    expect(mockOnClose).toHaveBeenCalled()
  })

  it('can navigate back from reauth to warning', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.click(screen.getByRole('button', { name: 'Back' }))
    expect(screen.getByText('This action is permanent')).toBeDefined()
  })

  it('can navigate back from confirm to reauth', () => {
    render(<AccountDeletionWizard onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }))
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.click(screen.getByRole('button', { name: 'Back' }))
    expect(screen.getByLabelText('Password')).toBeDefined()
  })
})
