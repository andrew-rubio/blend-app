import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import NotFoundPage from '@/app/not-found'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}))

describe('NotFoundPage', () => {
  it('renders 404 status code', () => {
    render(<NotFoundPage />)
    expect(screen.getByText('404')).toBeDefined()
  })

  it('renders page not found heading', () => {
    render(<NotFoundPage />)
    expect(screen.getByText('Page not found')).toBeDefined()
  })

  it('renders return home link', () => {
    render(<NotFoundPage />)
    const homeLink = screen.getByRole('link', { name: /return home/i })
    expect(homeLink).toBeDefined()
  })
})
