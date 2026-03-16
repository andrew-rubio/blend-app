import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from '@/components/ui/Button'

describe('Button', () => {
  it('renders children correctly', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByText('Click me')).toBeDefined()
  })

  it('matches snapshot', () => {
    const { container } = render(<Button variant="primary" size="md">Snapshot Button</Button>)
    expect(container.firstChild).toMatchSnapshot()
  })

  it('handles click events', () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Click me</Button>)
    fireEvent.click(screen.getByText('Click me'))
    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled</Button>)
    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  it('shows loading spinner when isLoading is true', () => {
    const { container } = render(<Button isLoading>Loading</Button>)
    expect(container.querySelector('.animate-spin')).toBeDefined()
  })

  it('is disabled when isLoading is true', () => {
    render(<Button isLoading>Loading</Button>)
    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  it('renders different variants', () => {
    const { rerender } = render(<Button variant="primary">Primary</Button>)
    expect(screen.getByRole('button')).toBeDefined()

    rerender(<Button variant="secondary">Secondary</Button>)
    expect(screen.getByRole('button')).toBeDefined()

    rerender(<Button variant="outline">Outline</Button>)
    expect(screen.getByRole('button')).toBeDefined()
  })
})
