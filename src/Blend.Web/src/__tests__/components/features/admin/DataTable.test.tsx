import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { DataTable } from '@/components/features/admin/DataTable'

interface TestRow {
  id: string
  name: string
  value: number
}

const mockData: TestRow[] = [
  { id: '1', name: 'Alpha', value: 10 },
  { id: '2', name: 'Beta', value: 20 },
  { id: '3', name: 'Gamma', value: 30 },
]

const columns = [
  { key: 'name', header: 'Name', render: (row: TestRow) => <span>{row.name}</span> },
  { key: 'value', header: 'Value', render: (row: TestRow) => <span>{row.value}</span> },
]

describe('DataTable', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders column headers', () => {
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
      />
    )
    expect(screen.getByText('Name')).toBeDefined()
    expect(screen.getByText('Value')).toBeDefined()
  })

  it('renders all data rows', () => {
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
      />
    )
    expect(screen.getByText('Alpha')).toBeDefined()
    expect(screen.getByText('Beta')).toBeDefined()
    expect(screen.getByText('Gamma')).toBeDefined()
  })

  it('shows empty message when data is empty', () => {
    render(
      <DataTable
        columns={columns}
        data={[]}
        keyExtractor={(row) => row.id}
        emptyMessage="Nothing here."
      />
    )
    expect(screen.getByText('Nothing here.')).toBeDefined()
  })

  it('shows loading skeleton when isLoading is true', () => {
    const { container } = render(
      <DataTable
        columns={columns}
        data={[]}
        keyExtractor={(row) => row.id}
        isLoading
      />
    )
    const skeletons = container.querySelectorAll('.animate-pulse')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders select-all checkbox when onRowSelect is provided', () => {
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
        onRowSelect={vi.fn()}
        selectedIds={[]}
      />
    )
    const selectAll = screen.getByLabelText('Select all')
    expect(selectAll).toBeDefined()
  })

  it('calls onRowSelect with all ids when select-all is checked', () => {
    const onRowSelect = vi.fn()
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
        onRowSelect={onRowSelect}
        selectedIds={[]}
      />
    )
    fireEvent.click(screen.getByLabelText('Select all'))
    expect(onRowSelect).toHaveBeenCalledWith(['1', '2', '3'])
  })

  it('calls onRowSelect with empty array when all already selected and select-all clicked', () => {
    const onRowSelect = vi.fn()
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
        onRowSelect={onRowSelect}
        selectedIds={['1', '2', '3']}
      />
    )
    fireEvent.click(screen.getByLabelText('Select all'))
    expect(onRowSelect).toHaveBeenCalledWith([])
  })

  it('calls onRowSelect with toggled id when individual row checkbox is clicked', () => {
    const onRowSelect = vi.fn()
    render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
        onRowSelect={onRowSelect}
        selectedIds={[]}
      />
    )
    fireEvent.click(screen.getByLabelText('Select row 1'))
    expect(onRowSelect).toHaveBeenCalledWith(['1'])
  })

  it('highlights selected rows', () => {
    const { container } = render(
      <DataTable
        columns={columns}
        data={mockData}
        keyExtractor={(row) => row.id}
        onRowSelect={vi.fn()}
        selectedIds={['2']}
      />
    )
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[1].className).toContain('bg-primary-50')
  })
})
