import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import IngredientsPage from '@/app/(admin)/ingredients/page'
import type { AdminIngredientSubmission, AdminIngredientSubmissionsResponse } from '@/types'

vi.mock('@/hooks/useAdmin', () => ({
  useAdminSubmissions: vi.fn(),
  useApproveSubmission: vi.fn(),
  useRejectSubmission: vi.fn(),
  useBatchApproveSubmissions: vi.fn(),
  useBatchRejectSubmissions: vi.fn(),
}))

import {
  useAdminSubmissions,
  useApproveSubmission,
  useRejectSubmission,
  useBatchApproveSubmissions,
  useBatchRejectSubmissions,
} from '@/hooks/useAdmin'

const mockUseSubmissions = vi.mocked(useAdminSubmissions)
const mockUseApprove = vi.mocked(useApproveSubmission)
const mockUseReject = vi.mocked(useRejectSubmission)
const mockUseBatchApprove = vi.mocked(useBatchApproveSubmissions)
const mockUseBatchReject = vi.mocked(useBatchRejectSubmissions)

const mockSubmissions: AdminIngredientSubmission[] = [
  {
    id: 'sub1',
    name: 'Turmeric',
    category: 'Spices',
    status: 'Pending',
    submittedById: 'u1',
    submittedByName: 'Alice',
    submittedAt: '2024-01-15T00:00:00Z',
  },
  {
    id: 'sub2',
    name: 'Quinoa',
    category: 'Grains',
    status: 'Pending',
    submittedById: 'u2',
    submittedByName: 'Bob',
    submittedAt: '2024-01-16T00:00:00Z',
  },
]

const mockResponse: AdminIngredientSubmissionsResponse = {
  submissions: mockSubmissions,
  total: 2,
  page: 1,
  pageSize: 20,
  totalPages: 1,
}

const noopMutate = vi.fn()

function setupMocks() {
  mockUseSubmissions.mockReturnValue({
    data: mockResponse,
    isLoading: false,
    error: null,
  } as ReturnType<typeof useAdminSubmissions>)
  mockUseApprove.mockReturnValue({
    mutate: noopMutate,
    isPending: false,
    variables: undefined,
  } as unknown as ReturnType<typeof useApproveSubmission>)
  mockUseReject.mockReturnValue({
    mutate: noopMutate,
    isPending: false,
  } as unknown as ReturnType<typeof useRejectSubmission>)
  mockUseBatchApprove.mockReturnValue({
    mutate: noopMutate,
    isPending: false,
  } as unknown as ReturnType<typeof useBatchApproveSubmissions>)
  mockUseBatchReject.mockReturnValue({
    mutate: noopMutate,
    isPending: false,
  } as unknown as ReturnType<typeof useBatchRejectSubmissions>)
}

describe('IngredientsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    setupMocks()
  })

  it('renders page heading', () => {
    render(<IngredientsPage />)
    expect(screen.getByText('Ingredient Submissions')).toBeDefined()
  })

  it('renders status filter tabs', () => {
    render(<IngredientsPage />)
    expect(screen.getByText('Pending')).toBeDefined()
    expect(screen.getByText('Approved')).toBeDefined()
    expect(screen.getByText('Rejected')).toBeDefined()
  })

  it('renders submission rows', () => {
    render(<IngredientsPage />)
    expect(screen.getByText('Turmeric')).toBeDefined()
    expect(screen.getByText('Quinoa')).toBeDefined()
    expect(screen.getByText('Alice')).toBeDefined()
    expect(screen.getByText('Bob')).toBeDefined()
  })

  it('renders Approve and Reject buttons for each pending submission', () => {
    render(<IngredientsPage />)
    const approveButtons = screen.getAllByText('Approve')
    const rejectButtons = screen.getAllByText('Reject')
    expect(approveButtons.length).toBe(2)
    expect(rejectButtons.length).toBe(2)
  })

  it('calls approve mutation when Approve button is clicked', () => {
    render(<IngredientsPage />)
    fireEvent.click(screen.getAllByText('Approve')[0])
    expect(noopMutate).toHaveBeenCalledWith({ id: 'sub1' })
  })

  it('shows reject dialog when Reject button is clicked', () => {
    render(<IngredientsPage />)
    fireEvent.click(screen.getAllByText('Reject')[0])
    expect(screen.getByRole('dialog', { name: 'Reject submission' })).toBeDefined()
    expect(screen.getAllByText('Turmeric').length).toBeGreaterThan(0)
  })

  it('closes reject dialog when Cancel is clicked', () => {
    render(<IngredientsPage />)
    fireEvent.click(screen.getAllByText('Reject')[0])
    expect(screen.getByRole('dialog')).toBeDefined()
    fireEvent.click(screen.getByText('Cancel'))
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('shows batch action bar when rows are selected', () => {
    render(<IngredientsPage />)
    fireEvent.click(screen.getByLabelText('Select all'))
    expect(screen.getByText(/2 selected/)).toBeDefined()
    expect(screen.getByText('Approve Selected')).toBeDefined()
    expect(screen.getByText('Reject Selected')).toBeDefined()
  })

  it('shows loading skeleton when data is loading', () => {
    mockUseSubmissions.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as ReturnType<typeof useAdminSubmissions>)

    const { container } = render(<IngredientsPage />)
    const skeletons = container.querySelectorAll('.animate-pulse')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('shows empty message when no submissions', () => {
    mockUseSubmissions.mockReturnValue({
      data: { submissions: [], total: 0, page: 1, pageSize: 20, totalPages: 1 },
      isLoading: false,
      error: null,
    } as ReturnType<typeof useAdminSubmissions>)

    render(<IngredientsPage />)
    expect(screen.getByText('No pending submissions.')).toBeDefined()
  })

  it('switches to Approved tab when clicked', () => {
    render(<IngredientsPage />)
    const approvedTab = screen.getByRole('tab', { name: 'Approved' })
    fireEvent.click(approvedTab)
    expect(approvedTab.getAttribute('aria-selected')).toBe('true')
  })
})
