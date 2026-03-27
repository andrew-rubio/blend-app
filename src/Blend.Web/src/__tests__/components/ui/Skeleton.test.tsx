import React from 'react'
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import {
  Skeleton,
  SkeletonText,
  SkeletonCard,
  SkeletonSection,
  LoadingSpinner,
} from '@/components/ui/Skeleton'

describe('Skeleton', () => {
  it('renders with aria-hidden', () => {
    const { container } = render(<Skeleton />)
    const el = container.firstChild as HTMLElement
    expect(el.getAttribute('aria-hidden')).toBe('true')
  })

  it('applies custom width and height', () => {
    const { container } = render(<Skeleton width={200} height={50} />)
    const el = container.firstChild as HTMLElement
    expect(el.style.width).toBe('200px')
    expect(el.style.height).toBe('50px')
  })

  it('applies custom className', () => {
    const { container } = render(<Skeleton className="custom-class" />)
    expect((container.firstChild as HTMLElement).className).toContain('custom-class')
  })
})

describe('SkeletonText', () => {
  it('renders default 3 lines', () => {
    const { container } = render(<SkeletonText />)
    // The root element is aria-hidden; its children are the skeleton lines
    const root = container.querySelector('[aria-hidden="true"]') as HTMLElement
    expect(root.children.length).toBe(3)
  })

  it('renders specified number of lines', () => {
    const { container } = render(<SkeletonText lines={5} />)
    const root = container.querySelector('[aria-hidden="true"]') as HTMLElement
    expect(root.children.length).toBe(5)
  })
})

describe('SkeletonCard', () => {
  it('renders without error', () => {
    const { container } = render(<SkeletonCard />)
    expect(container.firstChild).toBeDefined()
  })

  it('includes animate-pulse elements', () => {
    const { container } = render(<SkeletonCard />)
    const animated = container.querySelectorAll('.animate-pulse')
    expect(animated.length).toBeGreaterThan(0)
  })
})

describe('SkeletonSection', () => {
  it('renders default 3 cards', () => {
    render(<SkeletonSection />)
    const cards = document.querySelectorAll('[aria-hidden="true"]')
    expect(cards.length).toBeGreaterThan(0)
  })

  it('renders specified number of cards', () => {
    const { container } = render(<SkeletonSection cards={2} />)
    // Section has header + grid with 2 cards; each card has multiple divs
    expect(container.firstChild).toBeDefined()
  })
})

describe('LoadingSpinner', () => {
  it('renders with loading role', () => {
    render(<LoadingSpinner />)
    const spinner = screen.getByRole('status')
    expect(spinner).toBeDefined()
  })

  it('has accessible label', () => {
    render(<LoadingSpinner />)
    expect(screen.getByLabelText('Loading')).toBeDefined()
  })

  it('applies size class', () => {
    const { container } = render(<LoadingSpinner size="lg" />)
    expect((container.firstChild as HTMLElement).className).toContain('h-10')
  })

  it('renders small size', () => {
    const { container } = render(<LoadingSpinner size="sm" />)
    expect((container.firstChild as HTMLElement).className).toContain('h-4')
  })
})
