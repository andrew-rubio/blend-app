import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { IngredientTypeahead } from '@/components/features/preferences/IngredientTypeahead'

describe('IngredientTypeahead', () => {
  it('renders the search input', () => {
    render(
      <IngredientTypeahead addedIds={[]} onAdd={vi.fn()} onRemove={vi.fn()} />
    )
    expect(screen.getByLabelText('Add disliked ingredient')).toBeDefined()
  })

  it('renders added ingredients as chips', () => {
    render(
      <IngredientTypeahead addedIds={['cilantro', 'mushroom']} onAdd={vi.fn()} onRemove={vi.fn()} />
    )
    expect(screen.getByText('cilantro')).toBeDefined()
    expect(screen.getByText('mushroom')).toBeDefined()
  })

  it('calls onAdd when Enter is pressed with a value', async () => {
    const onAdd = vi.fn()
    render(<IngredientTypeahead addedIds={[]} onAdd={onAdd} onRemove={vi.fn()} />)
    const input = screen.getByLabelText('Add disliked ingredient')
    fireEvent.change(input, { target: { value: 'cilantro' } })
    fireEvent.keyDown(input, { key: 'Enter' })
    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith('cilantro')
    })
  })

  it('calls onAdd when Add button is clicked', async () => {
    const onAdd = vi.fn()
    render(<IngredientTypeahead addedIds={[]} onAdd={onAdd} onRemove={vi.fn()} />)
    const input = screen.getByLabelText('Add disliked ingredient')
    fireEvent.change(input, { target: { value: 'ginger' } })
    fireEvent.click(screen.getByLabelText('Add ingredient'))
    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith('ginger')
    })
  })

  it('trims and lowercases the input value before calling onAdd', async () => {
    const onAdd = vi.fn()
    render(<IngredientTypeahead addedIds={[]} onAdd={onAdd} onRemove={vi.fn()} />)
    const input = screen.getByLabelText('Add disliked ingredient')
    fireEvent.change(input, { target: { value: '  Cilantro  ' } })
    fireEvent.keyDown(input, { key: 'Enter' })
    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith('cilantro')
    })
  })

  it('does not call onAdd for empty input', async () => {
    const onAdd = vi.fn()
    render(<IngredientTypeahead addedIds={[]} onAdd={onAdd} onRemove={vi.fn()} />)
    const input = screen.getByLabelText('Add disliked ingredient')
    fireEvent.keyDown(input, { key: 'Enter' })
    expect(onAdd).not.toHaveBeenCalled()
  })

  it('does not call onAdd for duplicate ingredient', async () => {
    const onAdd = vi.fn()
    render(
      <IngredientTypeahead addedIds={['cilantro']} onAdd={onAdd} onRemove={vi.fn()} />
    )
    const input = screen.getByLabelText('Add disliked ingredient')
    fireEvent.change(input, { target: { value: 'cilantro' } })
    fireEvent.keyDown(input, { key: 'Enter' })
    expect(onAdd).not.toHaveBeenCalled()
  })

  it('calls onRemove when remove button is clicked for a chip', () => {
    const onRemove = vi.fn()
    render(
      <IngredientTypeahead addedIds={['cilantro']} onAdd={vi.fn()} onRemove={onRemove} />
    )
    fireEvent.click(screen.getByLabelText('Remove cilantro'))
    expect(onRemove).toHaveBeenCalledWith('cilantro')
  })

  it('shows empty state message when no ingredients are added', () => {
    render(<IngredientTypeahead addedIds={[]} onAdd={vi.fn()} onRemove={vi.fn()} />)
    expect(screen.getByText(/No ingredients added yet/)).toBeDefined()
  })

  it('Add button is disabled when input is empty', () => {
    render(<IngredientTypeahead addedIds={[]} onAdd={vi.fn()} onRemove={vi.fn()} />)
    expect(screen.getByLabelText('Add ingredient')).toBeDisabled()
  })
})
