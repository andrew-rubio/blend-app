import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CookButton } from '../CookButton'
import { useAuthStore } from '@/lib/stores/auth-store'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

describe('CookButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    useAuthStore.setState({ isAuthenticated: false, user: null })
  })

  it('renders cook button', () => {
    render(<CookButton recipeId="1" />)
    expect(screen.getByRole('button', { name: 'Cook this dish' })).toBeInTheDocument()
  })

  it('navigates to cook mode when authenticated', async () => {
    useAuthStore.setState({ isAuthenticated: true, user: { id: 'u1', name: 'User' } })
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: 'Cook this dish' }))
    expect(mockPush).toHaveBeenCalledWith('/cook/1')
  })

  it('shows login prompt when not authenticated', async () => {
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: 'Cook this dish' }))
    expect(screen.getByRole('dialog', { name: 'Login required' })).toBeInTheDocument()
    expect(screen.getByText('Sign in to cook')).toBeInTheDocument()
  })

  it('dismisses login prompt on cancel', async () => {
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: 'Cook this dish' }))
    await user.click(screen.getByRole('button', { name: /Cancel/ }))
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })
})
