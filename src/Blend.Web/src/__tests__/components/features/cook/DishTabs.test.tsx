import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { DishTabs } from '@/components/features/cook/DishTabs'
import type { CookingSessionDish } from '@/types'

const dishes: CookingSessionDish[] = [
  { dishId: 'dish-1', name: 'Pasta', ingredients: [] },
  { dishId: 'dish-2', name: 'Salad', ingredients: [] },
]

describe('DishTabs', () => {
  const mockOnSelect = vi.fn()
  const mockOnAdd = vi.fn()
  const mockOnRemove = vi.fn()
  const mockOnRename = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    vi.stubGlobal('confirm', vi.fn(() => true))
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders all dish tabs', () => {
    render(
      <DishTabs
        dishes={dishes}
        activeDishId="dish-1"
        onSelect={mockOnSelect}
        onAdd={mockOnAdd}
        onRemove={mockOnRemove}
        onRename={mockOnRename}
      />
    )
    expect(screen.getByText('Pasta')).toBeDefined()
    expect(screen.getByText('Salad')).toBeDefined()
  })

  it('calls onSelect when tab clicked', () => {
    render(
      <DishTabs
        dishes={dishes}
        activeDishId="dish-1"
        onSelect={mockOnSelect}
        onAdd={mockOnAdd}
        onRemove={mockOnRemove}
        onRename={mockOnRename}
      />
    )
    fireEvent.click(screen.getByTestId('dish-tab-select-dish-2'))
    expect(mockOnSelect).toHaveBeenCalledWith('dish-2')
  })

  it('calls onAdd when + button clicked', () => {
    render(
      <DishTabs
        dishes={dishes}
        activeDishId="dish-1"
        onSelect={mockOnSelect}
        onAdd={mockOnAdd}
        onRemove={mockOnRemove}
        onRename={mockOnRename}
      />
    )
    fireEvent.click(screen.getByTestId('dish-tab-add'))
    expect(mockOnAdd).toHaveBeenCalled()
  })

  it('calls onRemove when remove button clicked and confirmed', () => {
    render(
      <DishTabs
        dishes={dishes}
        activeDishId="dish-1"
        onSelect={mockOnSelect}
        onAdd={mockOnAdd}
        onRemove={mockOnRemove}
        onRename={mockOnRename}
      />
    )
    fireEvent.click(screen.getByTestId('dish-tab-remove-dish-2'))
    expect(mockOnRemove).toHaveBeenCalledWith('dish-2')
  })

  it('does not call onRemove when only one dish', () => {
    render(
      <DishTabs
        dishes={[dishes[0]]}
        activeDishId="dish-1"
        onSelect={mockOnSelect}
        onAdd={mockOnAdd}
        onRemove={mockOnRemove}
        onRename={mockOnRename}
      />
    )
    const removeBtn = screen.getByTestId('dish-tab-remove-dish-1')
    expect((removeBtn as HTMLButtonElement).disabled).toBe(true)
  })
})
