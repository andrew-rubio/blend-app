import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Header } from '@/components/layout/Header'

vi.mock('next/link', () => ({
  default: ({
    children,
    href,
    ...props
  }: {
    children: React.ReactNode
    href: string
    [key: string]: unknown
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}))

vi.mock('next/navigation', () => ({
  usePathname: vi.fn(),
  useRouter: vi.fn(),
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('@/lib/api/auth', () => ({
  logoutApi: vi.fn(),
}))

vi.mock('@/components/features/notifications/NotificationBell', () => ({
  NotificationBell: () => <div data-testid="notification-bell" />,
}))

vi.mock('@/components/ui/Button', () => ({
  Button: ({
    children,
    onClick,
    ...props
  }: {
    children: React.ReactNode
    onClick?: () => void
    [key: string]: unknown
  }) => (
    <button onClick={onClick} {...props}>
      {children}
    </button>
  ),
}))

import { usePathname, useRouter } from 'next/navigation'
import { useAuthStore } from '@/stores/authStore'

const mockUsePathname = vi.mocked(usePathname)
const mockUseRouter = vi.mocked(useRouter)
const mockUseAuthStore = vi.mocked(useAuthStore)

const mockPush = vi.fn()

beforeEach(() => {
  mockUsePathname.mockReturnValue('/home')
  mockUseRouter.mockReturnValue({ push: mockPush } as unknown as ReturnType<typeof useRouter>)
  mockUseAuthStore.mockReturnValue({
    isAuthenticated: false,
    user: null,
    token: null,
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
    setLoading: vi.fn(),
    updateUser: vi.fn(),
    setToken: vi.fn(),
  } as unknown as ReturnType<typeof useAuthStore>)
  mockPush.mockClear()
})

describe('Header — unauthenticated', () => {
  it('renders the header', () => {
    render(<Header />)
    expect(screen.getByRole('banner')).toBeDefined()
  })

  it('renders the Blend logo linking to /home', () => {
    render(<Header />)
    const logoLink = screen.getByRole('link', { name: 'Blend home' })
    expect(logoLink.getAttribute('href')).toBe('/home')
    expect(screen.getByText('Blend')).toBeDefined()
  })

  it('renders all 5 navigation links in the desktop nav', () => {
    render(<Header />)
    expect(screen.getByRole('link', { name: 'Home' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Explore' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Cook' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Friends' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Profile' })).toBeDefined()
  })

  it('shows Sign in and Get started buttons when not authenticated', () => {
    render(<Header />)
    expect(screen.getByText('Sign in')).toBeDefined()
    expect(screen.getByText('Get started')).toBeDefined()
  })

  it('does not show notification bell when not authenticated', () => {
    render(<Header />)
    expect(screen.queryByTestId('notification-bell')).toBeNull()
  })

  it('does not show avatar button when not authenticated', () => {
    render(<Header />)
    expect(screen.queryByTestId('avatar-button')).toBeNull()
  })
})

describe('Header — authenticated', () => {
  beforeEach(() => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: {
        id: '1',
        name: 'Jane Doe',
        email: 'jane@example.com',
        role: 'user',
        createdAt: '',
      },
      token: 'tok',
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    } as unknown as ReturnType<typeof useAuthStore>)
  })

  it('shows notification bell when authenticated', () => {
    render(<Header />)
    expect(screen.getByTestId('notification-bell')).toBeDefined()
  })

  it('shows avatar button with initials', () => {
    render(<Header />)
    expect(screen.getByTestId('avatar-button')).toBeDefined()
    expect(screen.getByText('JD')).toBeDefined()
  })

  it('does not show Sign in button when authenticated', () => {
    render(<Header />)
    expect(screen.queryByText('Sign in')).toBeNull()
  })

  it('opens avatar dropdown on click', () => {
    render(<Header />)
    const avatarButton = screen.getByTestId('avatar-button')
    fireEvent.click(avatarButton)
    expect(screen.getByTestId('avatar-dropdown')).toBeDefined()
  })

  it('dropdown shows user name, View Profile, Settings and Logout', () => {
    render(<Header />)
    fireEvent.click(screen.getByTestId('avatar-button'))
    expect(screen.getByText('Jane Doe')).toBeDefined()
    expect(screen.getByText('View Profile')).toBeDefined()
    expect(screen.getByText('Settings')).toBeDefined()
    expect(screen.getByText('Logout')).toBeDefined()
  })

  it('View Profile link points to /profile', () => {
    render(<Header />)
    fireEvent.click(screen.getByTestId('avatar-button'))
    const viewProfile = screen.getByText('View Profile')
    expect(viewProfile.closest('a')?.getAttribute('href')).toBe('/profile')
  })

  it('Settings link points to /settings', () => {
    render(<Header />)
    fireEvent.click(screen.getByTestId('avatar-button'))
    const settings = screen.getByText('Settings')
    expect(settings.closest('a')?.getAttribute('href')).toBe('/settings')
  })

  it('closes dropdown when avatar button clicked again', () => {
    render(<Header />)
    const avatarButton = screen.getByTestId('avatar-button')
    fireEvent.click(avatarButton)
    expect(screen.getByTestId('avatar-dropdown')).toBeDefined()
    fireEvent.click(avatarButton)
    expect(screen.queryByTestId('avatar-dropdown')).toBeNull()
  })

  it('dropdown closes after clicking View Profile', () => {
    render(<Header />)
    fireEvent.click(screen.getByTestId('avatar-button'))
    fireEvent.click(screen.getByText('View Profile'))
    expect(screen.queryByTestId('avatar-dropdown')).toBeNull()
  })
})

describe('Header — active state', () => {
  beforeEach(() => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: false,
      user: null,
      token: null,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    } as unknown as ReturnType<typeof useAuthStore>)
  })

  it('marks active nav link with aria-current="page" on /home', () => {
    mockUsePathname.mockReturnValue('/home')
    render(<Header />)
    const homeLink = screen.getByRole('link', { name: 'Home' })
    expect(homeLink.getAttribute('aria-current')).toBe('page')
  })

  it('marks active nav link with aria-current="page" on /explore', () => {
    mockUsePathname.mockReturnValue('/explore')
    render(<Header />)
    expect(screen.getByRole('link', { name: 'Explore' }).getAttribute('aria-current')).toBe('page')
  })

  it('uses prefix matching for active state', () => {
    mockUsePathname.mockReturnValue('/friends/requests')
    render(<Header />)
    expect(screen.getByRole('link', { name: 'Friends' }).getAttribute('aria-current')).toBe('page')
    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('aria-current')).toBeNull()
  })

  it('does not mark any nav link active on unrelated route', () => {
    mockUsePathname.mockReturnValue('/settings')
    render(<Header />)
    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('aria-current')).toBeNull()
    expect(screen.getByRole('link', { name: 'Explore' }).getAttribute('aria-current')).toBeNull()
  })
})

describe('Header — admin link', () => {
  it('shows admin link for admin users', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: {
        id: '2',
        name: 'Admin User',
        email: 'admin@example.com',
        role: 'admin',
        createdAt: '',
      },
      token: 'tok',
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    } as unknown as ReturnType<typeof useAuthStore>)
    render(<Header />)
    expect(screen.getByRole('link', { name: 'Admin' })).toBeDefined()
  })

  it('does not show admin link for regular users', () => {
    mockUseAuthStore.mockReturnValue({
      isAuthenticated: true,
      user: {
        id: '1',
        name: 'Jane Doe',
        email: 'jane@example.com',
        role: 'user',
        createdAt: '',
      },
      token: 'tok',
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      setLoading: vi.fn(),
      updateUser: vi.fn(),
      setToken: vi.fn(),
    } as unknown as ReturnType<typeof useAuthStore>)
    render(<Header />)
    expect(screen.queryByRole('link', { name: 'Admin' })).toBeNull()
  })
})
