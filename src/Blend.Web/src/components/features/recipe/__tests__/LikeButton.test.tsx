import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { LikeButton } from '../LikeButton'

vi.mock('@/lib/api/recipes', () => ({
  toggleLike: vi.fn().mockResolvedValue(undefined),
}))

import { toggleLike } from '@/lib/api/recipes'

describe('LikeButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders like count', () => {
    render(<LikeButton recipeId="1" initialLikes={42} initialIsLiked={false} />)
    expect(screen.getByText(/42/)).toBeInTheDocument()
  })

  it('toggles liked state optimistically on click', async () => {
    const user = userEvent.setup()
    render(<LikeButton recipeId="1" initialLikes={42} initialIsLiked={false} />)
    const btn = screen.getByRole('button')
    expect(btn).toHaveAttribute('aria-pressed', 'false')
    await user.click(btn)
    expect(btn).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByText(/43/)).toBeInTheDocument()
  })

  it('reverts like state when API call fails', async () => {
    vi.mocked(toggleLike).mockRejectedValueOnce(new Error('Network error'))
    const user = userEvent.setup()
    render(<LikeButton recipeId="1" initialLikes={42} initialIsLiked={false} />)
    const btn = screen.getByRole('button')
    await user.click(btn)
    await waitFor(() => {
      expect(btn).toHaveAttribute('aria-pressed', 'false')
      expect(screen.getByText(/42/)).toBeInTheDocument()
    })
  })

  it('unlikes when already liked', async () => {
    const user = userEvent.setup()
    render(<LikeButton recipeId="1" initialLikes={10} initialIsLiked={true} />)
    const btn = screen.getByRole('button')
    await user.click(btn)
    expect(btn).toHaveAttribute('aria-pressed', 'false')
    expect(screen.getByText(/9/)).toBeInTheDocument()
  })
})
