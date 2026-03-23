import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('next/link', () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) =>
    React.createElement('a', { href, ...props }, children),
}))

import { CompletionStep } from '@/components/features/cook/wrap-up/CompletionStep'

describe('CompletionStep', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows generic completion message when not published', () => {
    render(<CompletionStep publishedRecipeId={null} onReturnHome={vi.fn()} />)
    expect(screen.getByText('Session Complete!')).toBeDefined()
  })

  it('shows published message when recipe was published', () => {
    render(<CompletionStep publishedRecipeId="recipe-123" onReturnHome={vi.fn()} />)
    expect(screen.getByText('Recipe Published!')).toBeDefined()
  })

  it('shows link to published recipe', () => {
    render(<CompletionStep publishedRecipeId="recipe-123" onReturnHome={vi.fn()} />)
    const link = screen.getByTestId('view-recipe-link') as HTMLAnchorElement
    expect(link.href).toContain('/recipes/recipe-123')
  })

  it('does not show recipe link when not published', () => {
    render(<CompletionStep publishedRecipeId={null} onReturnHome={vi.fn()} />)
    expect(screen.queryByTestId('view-recipe-link')).toBeNull()
  })

  it('calls onReturnHome when button is clicked', () => {
    const onReturnHome = vi.fn()
    render(<CompletionStep publishedRecipeId={null} onReturnHome={onReturnHome} />)
    fireEvent.click(screen.getByTestId('return-home-button'))
    expect(onReturnHome).toHaveBeenCalledOnce()
  })
})
