import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'

vi.mock('@/hooks/useIngredientSubmissions', () => ({
  useCreateIngredientSubmission: vi.fn(),
}))

import { IngredientSubmissionForm } from '@/components/features/settings/IngredientSubmissionForm'
import { useCreateIngredientSubmission } from '@/hooks/useIngredientSubmissions'

const mockUseCreateIngredientSubmission = vi.mocked(useCreateIngredientSubmission)

describe('IngredientSubmissionForm', () => {
  const mockOnClose = vi.fn()
  const mockMutate = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseCreateIngredientSubmission.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useCreateIngredientSubmission>)
  })

  it('renders all form fields', () => {
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    expect(screen.getByLabelText(/Ingredient name/i)).toBeDefined()
    expect(screen.getByLabelText(/Category/i)).toBeDefined()
    expect(screen.getByLabelText(/Description/i)).toBeDefined()
  })

  it('shows validation error for empty name on submit', () => {
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Submit' }))
    expect(screen.getByRole('alert')).toBeDefined()
    expect(mockMutate).not.toHaveBeenCalled()
  })

  it('shows validation error for name that is too short', () => {
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    fireEvent.change(screen.getByLabelText(/Ingredient name/i), { target: { value: 'A' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit' }))
    expect(screen.getByRole('alert')).toBeDefined()
    expect(mockMutate).not.toHaveBeenCalled()
  })

  it('calls mutate with correct data on valid submit', () => {
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    fireEvent.change(screen.getByLabelText(/Ingredient name/i), { target: { value: 'Dragonfruit' } })
    fireEvent.change(screen.getByLabelText(/Description/i), { target: { value: 'A tropical fruit' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit' }))
    expect(mockMutate).toHaveBeenCalledWith(
      { name: 'Dragonfruit', category: 'Produce', description: 'A tropical fruit' },
      expect.any(Object)
    )
  })

  it('shows success screen after successful submission', () => {
    mockUseCreateIngredientSubmission.mockReturnValue({
      mutate: (_data: unknown, opts?: { onSuccess?: (result: unknown) => void }) => { opts?.onSuccess?.({}) },
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useCreateIngredientSubmission>)
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    fireEvent.change(screen.getByLabelText(/Ingredient name/i), { target: { value: 'Dragonfruit' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit' }))
    expect(screen.getByText('Submission received!')).toBeDefined()
  })

  it('calls onClose when Cancel is clicked', () => {
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(mockOnClose).toHaveBeenCalled()
  })

  it('shows API error message on failure', () => {
    mockUseCreateIngredientSubmission.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: { message: 'Server error', status: 500 },
    } as unknown as ReturnType<typeof useCreateIngredientSubmission>)
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    expect(screen.getByRole('alert')).toBeDefined()
    expect(screen.getByText('Server error')).toBeDefined()
  })

  it('disables buttons while pending', () => {
    mockUseCreateIngredientSubmission.mockReturnValue({
      mutate: mockMutate,
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useCreateIngredientSubmission>)
    render(<IngredientSubmissionForm onClose={mockOnClose} />)
    expect((screen.getByRole('button', { name: 'Submitting…' }) as HTMLButtonElement).disabled).toBe(true)
  })
})
