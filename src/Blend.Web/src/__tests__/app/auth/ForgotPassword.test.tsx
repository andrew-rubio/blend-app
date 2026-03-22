import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import React from 'react'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}))

const mockForgotPasswordApi = vi.fn()
vi.mock('@/lib/api/auth', () => ({
  forgotPasswordApi: (...args: unknown[]) => mockForgotPasswordApi(...args),
  resetPasswordApi: vi.fn(),
}))

import ForgotPasswordPage from '@/app/(auth)/forgot-password/page'

describe('ForgotPasswordPage', () => {
  beforeEach(() => {
    mockForgotPasswordApi.mockClear()
  })

  it('renders the forgot password form', () => {
    render(<ForgotPasswordPage />)
    expect(screen.getByText('Reset your password')).toBeDefined()
    expect(screen.getByLabelText('Email address')).toBeDefined()
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeDefined()
  })

  it('renders sign in link', () => {
    render(<ForgotPasswordPage />)
    expect(screen.getByText(/sign in/i)).toBeDefined()
  })

  it('shows email validation error on empty submit', async () => {
    render(<ForgotPasswordPage />)
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }))
    await waitFor(() => {
      expect(screen.getByText('Email is required')).toBeDefined()
    })
  })

  it('shows email format error for invalid email', async () => {
    render(<ForgotPasswordPage />)
    fireEvent.change(screen.getByLabelText('Email address'), { target: { value: 'notanemail' } })
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }))
    await waitFor(() => {
      expect(screen.getByText('Invalid email format')).toBeDefined()
    })
  })

  it('shows generic success message after submission regardless of response', async () => {
    mockForgotPasswordApi.mockResolvedValueOnce(undefined)

    render(<ForgotPasswordPage />)
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'test@example.com' },
    })
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }))

    await waitFor(() => {
      expect(screen.getByRole('status')).toBeDefined()
      expect(screen.getByText('Check your email')).toBeDefined()
    })
  })

  it('shows generic success message even when backend returns error (account enumeration prevention)', async () => {
    mockForgotPasswordApi.mockRejectedValueOnce({ message: 'User not found', status: 404 })

    render(<ForgotPasswordPage />)
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'nonexistent@example.com' },
    })
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }))

    await waitFor(() => {
      // Should still show success, not an error
      expect(screen.getByRole('status')).toBeDefined()
      expect(screen.getByText('Check your email')).toBeDefined()
      expect(screen.queryByRole('alert')).toBeNull()
    })
  })
})
