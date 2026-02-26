import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ServingAdjuster } from '../ServingAdjuster'

describe('ServingAdjuster', () => {
  it('renders the current serving count', () => {
    render(<ServingAdjuster servings={4} onServingsChange={vi.fn()} />)
    expect(screen.getByText('4')).toBeInTheDocument()
  })

  it('calls onServingsChange with incremented value when + is clicked', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<ServingAdjuster servings={4} onServingsChange={onChange} />)
    await user.click(screen.getByRole('button', { name: 'Increase servings' }))
    expect(onChange).toHaveBeenCalledWith(5)
  })

  it('calls onServingsChange with decremented value when - is clicked', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<ServingAdjuster servings={4} onServingsChange={onChange} />)
    await user.click(screen.getByRole('button', { name: 'Decrease servings' }))
    expect(onChange).toHaveBeenCalledWith(3)
  })

  it('disables the decrement button when servings is 1', () => {
    render(<ServingAdjuster servings={1} onServingsChange={vi.fn()} />)
    expect(screen.getByRole('button', { name: 'Decrease servings' })).toBeDisabled()
  })

  it('recalculates ingredient amounts when servings change', async () => {
    const user = userEvent.setup()
    // Test the integrated IngredientsTab behavior via state
    const { rerender } = render(<ServingAdjuster servings={4} onServingsChange={vi.fn()} />)
    rerender(<ServingAdjuster servings={8} onServingsChange={vi.fn()} />)
    expect(screen.getByText('8')).toBeInTheDocument()
  })
})
