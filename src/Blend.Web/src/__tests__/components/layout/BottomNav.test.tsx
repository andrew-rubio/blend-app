import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { BottomNav } from '@/components/layout/BottomNav'

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
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('@/components/features/GuestPromptModal', () => ({
  GuestPromptModal: ({
    isOpen,
    onClose,
    message,
  }: {
    isOpen: boolean
    onClose: () => void
    message?: string
  }) =>
    isOpen ? (
      <div data-testid="guest-prompt-modal">
        <span>{message ?? 'default'}</span>
        <button onClick={onClose}>Close</button>
      </div>
    ) : null,
  useGuestPrompt: vi.fn(),
}))

import { usePathname } from 'next/navigation'
import { useAuthStore } from '@/stores/authStore'
import { useGuestPrompt } from '@/components/features/GuestPromptModal'

const mockUsePathname = vi.mocked(usePathname)
const mockUseAuthStore = vi.mocked(useAuthStore)
const mockUseGuestPrompt = vi.mocked(useGuestPrompt)

const mockPrompt = vi.fn()
const mockClose = vi.fn()

beforeEach(() => {
  mockUsePathname.mockReturnValue('/home')
  mockUseAuthStore.mockReturnValue({
    isAuthenticated: true,
    user: { id: '1', name: 'Test User', email: 'test@example.com', role: 'user', createdAt: '' },
    token: 'tok',
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
    setLoading: vi.fn(),
    updateUser: vi.fn(),
    setToken: vi.fn(),
  } as unknown as ReturnType<typeof useAuthStore>)
  mockUseGuestPrompt.mockReturnValue({
    isOpen: false,
    message: undefined,
    prompt: mockPrompt,
    close: mockClose,
  })
  mockPrompt.mockClear()
  mockClose.mockClear()
})

describe('BottomNav', () => {
  it('renders the bottom navigation', () => {
    render(<BottomNav />)
    expect(screen.getByTestId('bottom-nav')).toBeDefined()
  })

  it('renders all 5 navigation items', () => {
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Home' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Explore' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Cook' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Friends' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Profile' })).toBeDefined()
  })

  it('renders all navigation link hrefs correctly', () => {
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('href')).toBe('/home')
    expect(screen.getByRole('link', { name: 'Explore' }).getAttribute('href')).toBe('/explore')
    expect(screen.getByRole('link', { name: 'Cook' }).getAttribute('href')).toBe('/cook')
    expect(screen.getByRole('link', { name: 'Friends' }).getAttribute('href')).toBe('/friends')
    expect(screen.getByRole('link', { name: 'Profile' }).getAttribute('href')).toBe('/profile')
  })

  it('marks active route with aria-current="page"', () => {
    mockUsePathname.mockReturnValue('/home')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('aria-current')).toBe('page')
  })

  it('does not mark inactive routes with aria-current', () => {
    mockUsePathname.mockReturnValue('/home')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Explore' }).getAttribute('aria-current')).toBeNull()
  })

  it('marks active state by route prefix (nested routes)', () => {
    mockUsePathname.mockReturnValue('/explore/recipe/123')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Explore' }).getAttribute('aria-current')).toBe('page')
  })

  it('marks cook as active when on /cook', () => {
    mockUsePathname.mockReturnValue('/cook')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Cook' }).getAttribute('aria-current')).toBe('page')
  })

  it('marks friends as active when on /friends', () => {
    mockUsePathname.mockReturnValue('/friends')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Friends' }).getAttribute('aria-current')).toBe('page')
  })

  it('marks profile as active when on /profile', () => {
    mockUsePathname.mockReturnValue('/profile')
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Profile' }).getAttribute('aria-current')).toBe('page')
  })

  it('shows label text for non-cook items', () => {
    render(<BottomNav />)
    expect(screen.getByText('Home')).toBeDefined()
    expect(screen.getByText('Explore')).toBeDefined()
    expect(screen.getByText('Friends')).toBeDefined()
    expect(screen.getByText('Profile')).toBeDefined()
  })
})

describe('BottomNav — guest access', () => {
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

  it('renders buttons instead of links for restricted tabs when not authenticated', () => {
    render(<BottomNav />)
    // Cook, Friends, Profile should be buttons (not links) for unauthenticated users
    const buttons = screen.getAllByRole('button')
    const buttonLabels = buttons.map((b) => b.getAttribute('aria-label'))
    expect(buttonLabels).toContain('Cook')
    expect(buttonLabels).toContain('Friends')
    expect(buttonLabels).toContain('Profile')
  })

  it('Home and Explore remain links for unauthenticated users', () => {
    render(<BottomNav />)
    expect(screen.getByRole('link', { name: 'Home' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Explore' })).toBeDefined()
  })

  it('clicking Cook tab when unauthenticated prompts guest modal', () => {
    render(<BottomNav />)
    const cookButton = screen.getByRole('button', { name: 'Cook' })
    fireEvent.click(cookButton)
    expect(mockPrompt).toHaveBeenCalledWith(
      'Sign in to access Cook Mode and start tracking your cooking sessions.'
    )
  })

  it('clicking Friends tab when unauthenticated prompts guest modal', () => {
    render(<BottomNav />)
    const friendsButton = screen.getByRole('button', { name: 'Friends' })
    fireEvent.click(friendsButton)
    expect(mockPrompt).toHaveBeenCalledWith(
      'Sign in to connect with friends and share your culinary journey.'
    )
  })

  it('clicking Profile tab when unauthenticated prompts guest modal', () => {
    render(<BottomNav />)
    const profileButton = screen.getByRole('button', { name: 'Profile' })
    fireEvent.click(profileButton)
    expect(mockPrompt).toHaveBeenCalledWith('Sign in to view and manage your profile.')
  })

  it('renders guest prompt modal when isOpen is true', () => {
    mockUseGuestPrompt.mockReturnValue({
      isOpen: true,
      message: 'Please sign in.',
      prompt: mockPrompt,
      close: mockClose,
    })
    render(<BottomNav />)
    expect(screen.getByTestId('guest-prompt-modal')).toBeDefined()
  })

  it('does not render guest prompt modal when isOpen is false', () => {
    render(<BottomNav />)
    expect(screen.queryByTestId('guest-prompt-modal')).toBeNull()
  })
})
