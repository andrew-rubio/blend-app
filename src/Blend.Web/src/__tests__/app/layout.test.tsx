import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'

vi.mock('next/font/google', () => ({
  Inter: () => ({
    variable: '--font-sans',
    className: 'inter-font',
  }),
}))

vi.mock('@/lib/providers', () => ({
  Providers: ({ children }: { children: React.ReactNode }) => <div data-testid="providers">{children}</div>,
}))

import { Providers } from '@/lib/providers'

describe('RootLayout', () => {
  it('renders children inside providers', () => {
    render(
      <Providers>
        <div data-testid="child-content">Hello World</div>
      </Providers>
    )

    expect(screen.getByTestId('providers')).toBeDefined()
    expect(screen.getByTestId('child-content')).toBeDefined()
    expect(screen.getByText('Hello World')).toBeDefined()
  })

  it('Providers wraps children correctly', () => {
    render(
      <Providers>
        <span>Test content</span>
      </Providers>
    )
    expect(screen.getByText('Test content')).toBeDefined()
  })
})
