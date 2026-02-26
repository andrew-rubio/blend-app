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
    // Reset auth state
    useAuthStore.setState({ isAuthenticated: false, user: null })
  })

  it('renders the cook button', () => {
    render(<CookButton recipeId="1" />)
    expect(screen.getByRole('button', { name: /Cook this dish/i })).toBeInTheDocument()
  })

  it('navigates to cook page for authenticated users', async () => {
    useAuthStore.setState({ isAuthenticated: true, user: { id: 'u1', name: 'Alice' } })
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: /Cook this dish/i }))
    expect(mockPush).toHaveBeenCalledWith('/cook/1')
  })

  it('shows login prompt for unauthenticated users', async () => {
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: /Cook this dish/i }))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText(/Sign in to cook/i)).toBeInTheDocument()
  })

  it('closes login prompt on cancel', async () => {
    const user = userEvent.setup()
    render(<CookButton recipeId="1" />)
    await user.click(screen.getByRole('button', { name: /Cook this dish/i }))
    await user.click(screen.getByRole('button', { name: /Cancel/i }))
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })
})
