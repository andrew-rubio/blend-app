import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { LikeButton } from '../LikeButton'

function wrapper({ children }: { children: React.ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { mutations: { retry: false }, queries: { retry: false } },
  })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('LikeButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    global.fetch = vi.fn().mockResolvedValue({ ok: true } as Response)
  })

  it('renders unlike state when liked', () => {
    render(<LikeButton recipeId="1" liked={true} likes={10} />, { wrapper })
    expect(screen.getByRole('button', { name: 'Unlike recipe' })).toBeInTheDocument()
  })

  it('renders like state when not liked', () => {
    render(<LikeButton recipeId="1" liked={false} likes={9} />, { wrapper })
    expect(screen.getByRole('button', { name: 'Like recipe' })).toBeInTheDocument()
  })

  it('has aria-pressed reflecting the liked state', () => {
    render(<LikeButton recipeId="1" liked={true} likes={10} />, { wrapper })
    expect(screen.getByRole('button', { name: 'Unlike recipe' })).toHaveAttribute('aria-pressed', 'true')
  })

  it('triggers mutation on click', async () => {
    const user = userEvent.setup()
    render(<LikeButton recipeId="1" liked={false} likes={9} />, { wrapper })
    await user.click(screen.getByRole('button', { name: 'Like recipe' }))
    expect(global.fetch).toHaveBeenCalled()
  })
})
