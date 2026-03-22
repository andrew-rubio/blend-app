import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import ErrorPage from '@/app/error'

describe('ErrorPage', () => {
  it('renders error message', () => {
    const mockError = new Error('Test error message')
    const mockReset = vi.fn()

    render(<ErrorPage error={mockError} reset={mockReset} />)

    expect(screen.getByText('Something went wrong')).toBeDefined()
    expect(screen.getByText('Test error message')).toBeDefined()
  })

  it('renders default error message when no message provided', () => {
    const mockError = Object.assign(new Error(), { message: '' })
    const mockReset = vi.fn()

    render(<ErrorPage error={mockError} reset={mockReset} />)

    expect(screen.getByText('An unexpected error occurred. Please try again.')).toBeDefined()
  })

  it('calls reset when Try again button is clicked', () => {
    const mockError = new Error('Test error')
    const mockReset = vi.fn()

    render(<ErrorPage error={mockError} reset={mockReset} />)

    const button = screen.getByText('Try again')
    fireEvent.click(button)

    expect(mockReset).toHaveBeenCalledTimes(1)
  })
})
