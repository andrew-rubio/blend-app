import React from 'react'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import { DishNotes } from '@/components/features/cook/DishNotes'

describe('DishNotes', () => {
  const mockOnSave = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('renders textarea with initial notes', () => {
    render(<DishNotes notes="some notes" onSave={mockOnSave} />)
    const textarea = screen.getByTestId('dish-notes-textarea') as HTMLTextAreaElement
    expect(textarea.value).toBe('some notes')
  })

  it('does not call onSave on first render', () => {
    render(<DishNotes notes="initial" onSave={mockOnSave} />)
    act(() => { vi.advanceTimersByTime(600) })
    expect(mockOnSave).not.toHaveBeenCalled()
  })

  it('calls onSave after debounce when value changes', () => {
    render(<DishNotes notes="" onSave={mockOnSave} />)
    fireEvent.change(screen.getByTestId('dish-notes-textarea'), { target: { value: 'new note' } })
    act(() => { vi.advanceTimersByTime(600) })
    expect(mockOnSave).toHaveBeenCalledWith('new note')
  })

  it('shows Saved indicator after save', () => {
    render(<DishNotes notes="" onSave={mockOnSave} />)
    fireEvent.change(screen.getByTestId('dish-notes-textarea'), { target: { value: 'hello' } })
    act(() => { vi.advanceTimersByTime(600) })
    expect(screen.getByTestId('dish-notes-saved')).toBeDefined()
  })

  it('updates textarea when notes prop changes', () => {
    const { rerender } = render(<DishNotes notes="old" onSave={mockOnSave} />)
    rerender(<DishNotes notes="updated externally" onSave={mockOnSave} />)
    const textarea = screen.getByTestId('dish-notes-textarea') as HTMLTextAreaElement
    expect(textarea.value).toBe('updated externally')
  })

  it('disables textarea when disabled prop set', () => {
    render(<DishNotes notes="" onSave={mockOnSave} disabled />)
    const textarea = screen.getByTestId('dish-notes-textarea') as HTMLTextAreaElement
    expect(textarea.disabled).toBe(true)
  })
})
