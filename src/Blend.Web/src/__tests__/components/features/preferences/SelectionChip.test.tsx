import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SelectionChip } from '@/components/features/preferences/SelectionChip'

describe('SelectionChip', () => {
  it('renders the label', () => {
    render(<SelectionChip label="Italian" selected={false} onClick={vi.fn()} />)
    expect(screen.getByText('Italian')).toBeDefined()
  })

  it('calls onClick when clicked', () => {
    const onClick = vi.fn()
    render(<SelectionChip label="Italian" selected={false} onClick={onClick} />)
    fireEvent.click(screen.getByRole('checkbox', { name: 'Italian' }))
    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('shows checkmark when selected', () => {
    const { container } = render(<SelectionChip label="Italian" selected={true} onClick={vi.fn()} />)
    expect(container.querySelector('svg')).toBeDefined()
  })

  it('does not show checkmark when not selected', () => {
    const { container } = render(
      <SelectionChip label="Italian" selected={false} onClick={vi.fn()} />
    )
    expect(container.querySelector('svg')).toBeNull()
  })

  it('has aria-checked=true when selected', () => {
    render(<SelectionChip label="Italian" selected={true} onClick={vi.fn()} />)
    expect(screen.getByRole('checkbox', { name: 'Italian' }).getAttribute('aria-checked')).toBe(
      'true'
    )
  })

  it('has aria-checked=false when not selected', () => {
    render(<SelectionChip label="Italian" selected={false} onClick={vi.fn()} />)
    expect(screen.getByRole('checkbox', { name: 'Italian' }).getAttribute('aria-checked')).toBe(
      'false'
    )
  })

  it('is disabled when disabled prop is true', () => {
    render(<SelectionChip label="Italian" selected={false} onClick={vi.fn()} disabled />)
    expect(screen.getByRole('checkbox', { name: 'Italian' })).toBeDisabled()
  })

  it('does not call onClick when disabled', () => {
    const onClick = vi.fn()
    render(<SelectionChip label="Italian" selected={false} onClick={onClick} disabled />)
    fireEvent.click(screen.getByRole('checkbox', { name: 'Italian' }))
    expect(onClick).not.toHaveBeenCalled()
  })
})
