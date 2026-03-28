import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import React from 'react'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}))

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

const mockLogin = vi.fn()
vi.mock('@/stores/authStore', () => ({
  useAuthStore: () => ({ login: mockLogin }),
}))

const mockLoginApi = vi.fn()
vi.mock('@/lib/api/auth', () => ({
  loginApi: (...args: unknown[]) => mockLoginApi(...args),
  getSocialLoginUrl: (provider: string) => `http://localhost:5000/api/v1/auth/${provider}`,
}))

import LoginPage from '@/app/(auth)/login/page'

describe('LoginPage', () => {
  beforeEach(() => {
    mockPush.mockClear()
    mockLogin.mockClear()
    mockLoginApi.mockClear()
  })

  it('renders the sign in form', () => {
    render(<LoginPage />)
    expect(screen.getByText('Sign in to Blend')).toBeDefined()
    expect(screen.getByLabelText('Email address')).toBeDefined()
    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeDefined()
  })

  it('renders forgot password link', () => {
    render(<LoginPage />)
    const link = screen.getByText('Forgot your password?')
    expect(link).toBeDefined()
    expect((link as HTMLAnchorElement).href).toContain('/forgot-password')
  })

  it('renders social login buttons', () => {
    render(<LoginPage />)
    expect(screen.getByTestId('social-login-google')).toBeDefined()
    expect(screen.getByTestId('social-login-facebook')).toBeDefined()
    expect(screen.getByTestId('social-login-twitter')).toBeDefined()
  })

  it('renders register link', () => {
    render(<LoginPage />)
    expect(screen.getByText(/sign up/i)).toBeDefined()
  })

  it('shows validation errors when submitted empty', async () => {
    render(<LoginPage />)
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))
    await waitFor(() => {
      expect(screen.getByText('Email is required')).toBeDefined()
      expect(screen.getByText('Password is required')).toBeDefined()
    })
  })

  it('shows email format error for invalid email', async () => {
    render(<LoginPage />)
    fireEvent.change(screen.getByLabelText('Email address'), { target: { value: 'notanemail' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))
    await waitFor(() => {
      expect(screen.getByText('Invalid email format')).toBeDefined()
    })
  })

  it('calls loginApi with credentials on valid submit', async () => {
    mockLoginApi.mockResolvedValueOnce({
      token: 'test-token',
      user: { id: '1', email: 'test@example.com', name: 'Test', role: 'user', createdAt: '' },
    })

    render(<LoginPage />)
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'mypassword' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(mockLoginApi).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'mypassword',
      })
      expect(mockLogin).toHaveBeenCalledTimes(1)
      expect(mockPush).toHaveBeenCalledWith('/home')
    })
  })

  it('displays error message on login failure', async () => {
    mockLoginApi.mockRejectedValueOnce({ message: 'Invalid credentials', status: 401 })

    render(<LoginPage />)
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'wrongpassword' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeDefined()
      expect(screen.getByText('Invalid credentials')).toBeDefined()
    })
  })
})
