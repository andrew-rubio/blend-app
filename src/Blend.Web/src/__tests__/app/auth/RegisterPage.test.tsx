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

const mockRegisterApi = vi.fn()
vi.mock('@/lib/api/auth', () => ({
  registerApi: (...args: unknown[]) => mockRegisterApi(...args),
  getSocialLoginUrl: (provider: string) => `http://localhost:5000/api/auth/${provider}`,
}))

import RegisterPage from '@/app/(auth)/register/page'

describe('RegisterPage', () => {
  beforeEach(() => {
    mockPush.mockClear()
    mockLogin.mockClear()
    mockRegisterApi.mockClear()
  })

  it('renders the registration form', () => {
    render(<RegisterPage />)
    expect(screen.getByText('Create your account')).toBeDefined()
    expect(screen.getByLabelText('Display name')).toBeDefined()
    expect(screen.getByLabelText('Email address')).toBeDefined()
    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(screen.getByLabelText('Confirm password')).toBeDefined()
  })

  it('renders social login buttons', () => {
    render(<RegisterPage />)
    expect(screen.getByTestId('social-login-google')).toBeDefined()
    expect(screen.getByTestId('social-login-facebook')).toBeDefined()
    expect(screen.getByTestId('social-login-twitter')).toBeDefined()
  })

  it('renders sign in link', () => {
    render(<RegisterPage />)
    expect(screen.getByText(/sign in/i)).toBeDefined()
  })

  it('renders skip preferences button', () => {
    render(<RegisterPage />)
    expect(screen.getByText(/skip preferences/i)).toBeDefined()
  })

  it('shows validation errors when submitted empty', async () => {
    render(<RegisterPage />)
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))
    await waitFor(() => {
      expect(screen.getByText('Display name is required')).toBeDefined()
      expect(screen.getByText('Email is required')).toBeDefined()
      expect(screen.getByText('Password does not meet the requirements')).toBeDefined()
    })
  })

  it('shows password strength indicator when typing', async () => {
    render(<RegisterPage />)
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'short' } })
    await waitFor(() => {
      expect(screen.getByText('Too short')).toBeDefined()
    })
  })

  it('shows password requirements list', async () => {
    render(<RegisterPage />)
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'test' } })
    await waitFor(() => {
      expect(screen.getByText(/at least 8 characters/i)).toBeDefined()
      expect(screen.getByText(/one uppercase letter/i)).toBeDefined()
    })
  })

  it('shows confirm password mismatch error', async () => {
    render(<RegisterPage />)
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'Password1!' } })
    fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'Different1!' } })
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))
    await waitFor(() => {
      expect(screen.getByText('Passwords do not match')).toBeDefined()
    })
  })

  it('calls registerApi on valid submit', async () => {
    mockRegisterApi.mockResolvedValueOnce({
      token: 'test-token',
      user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'user', createdAt: '' },
    })

    render(<RegisterPage />)
    fireEvent.change(screen.getByLabelText('Display name'), { target: { value: 'Test User' } })
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'Password1!' } })
    fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'Password1!' } })
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(mockRegisterApi).toHaveBeenCalledWith({
        name: 'Test User',
        email: 'test@example.com',
        password: 'Password1!',
      })
      expect(mockLogin).toHaveBeenCalledTimes(1)
      expect(mockPush).toHaveBeenCalledWith('/preferences')
    })
  })

  it('displays error message on registration failure', async () => {
    mockRegisterApi.mockRejectedValueOnce({ message: 'Email already in use', status: 409 })

    render(<RegisterPage />)
    fireEvent.change(screen.getByLabelText('Display name'), { target: { value: 'Test User' } })
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'Password1!' } })
    fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'Password1!' } })
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeDefined()
      expect(screen.getByText('Email already in use')).toBeDefined()
    })
  })

  it('navigates to home when skip preferences is clicked', async () => {
    render(<RegisterPage />)
    fireEvent.click(screen.getByText(/skip preferences/i))
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/home')
    })
  })
})
