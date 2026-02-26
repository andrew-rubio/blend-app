import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ShareButton } from '../ShareButton'

beforeEach(() => {
  vi.clearAllMocks()
  Object.defineProperty(window, 'location', {
    value: { href: 'http://localhost/recipes/1' },
    writable: true,
    configurable: true,
  })
})

describe('ShareButton', () => {
  it('renders share button', () => {
    render(<ShareButton />)
    expect(screen.getByRole('button', { name: 'Share recipe' })).toBeInTheDocument()
  })

  it('copies URL to clipboard on click and shows feedback', async () => {
    const user = userEvent.setup()
    render(<ShareButton />)
    await user.click(screen.getByRole('button', { name: 'Share recipe' }))
    await waitFor(() => {
      expect(screen.getByText(/Copied!/)).toBeInTheDocument()
    })
  })

  it('shows copied feedback after clicking', async () => {
    const user = userEvent.setup()
    render(<ShareButton />)
    await user.click(screen.getByRole('button', { name: 'Share recipe' }))
    await waitFor(() => {
      expect(screen.getByText(/Copied!/)).toBeInTheDocument()
    })
  })
})
