import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FeaturedVideosSection, FeaturedVideosSkeleton } from '@/components/features/home/FeaturedVideosSection'
import type { HomeFeaturedVideo } from '@/types'

vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockVideos: HomeFeaturedVideo[] = [
  { id: 'v1', title: 'How to Make Ramen', creator: 'Chef Ken', thumbnailUrl: 'https://example.com/ramen.jpg', videoUrl: 'https://example.com/ramen.mp4' },
  { id: 'v2', title: 'Pizza Secrets' },
]

describe('FeaturedVideosSection', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns null when videos list is empty', () => {
    const { container } = render(<FeaturedVideosSection videos={[]} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders section heading', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    expect(screen.getByText('Featured Videos')).toBeDefined()
  })

  it('renders video titles', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    expect(screen.getByText('How to Make Ramen')).toBeDefined()
    expect(screen.getByText('Pizza Secrets')).toBeDefined()
  })

  it('renders creator name', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    expect(screen.getByText('Chef Ken')).toBeDefined()
  })

  it('opens video player on click', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    const article = screen.getByRole('article', { name: 'Play video: How to Make Ramen' })
    fireEvent.click(article)
    expect(screen.getByRole('dialog')).toBeDefined()
  })

  it('closes video player when close button is clicked', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    const article = screen.getByRole('article', { name: 'Play video: How to Make Ramen' })
    fireEvent.click(article)
    const closeBtn = screen.getByLabelText('Close video player')
    fireEvent.click(closeBtn)
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('closes video player when backdrop is clicked', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    const article = screen.getByRole('article', { name: 'Play video: How to Make Ramen' })
    fireEvent.click(article)
    const dialog = screen.getByRole('dialog')
    fireEvent.click(dialog)
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('opens via keyboard Enter', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    const article = screen.getByRole('article', { name: 'Play video: How to Make Ramen' })
    fireEvent.keyDown(article, { key: 'Enter' })
    expect(screen.getByRole('dialog')).toBeDefined()
  })

  it('opens via keyboard Space', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    const article = screen.getByRole('article', { name: 'Play video: How to Make Ramen' })
    fireEvent.keyDown(article, { key: ' ' })
    expect(screen.getByRole('dialog')).toBeDefined()
  })

  it('has correct list role', () => {
    render(<FeaturedVideosSection videos={mockVideos} />)
    expect(screen.getByRole('list', { name: 'Featured videos' })).toBeDefined()
  })

  it('shows video unavailable when no videoUrl', () => {
    render(<FeaturedVideosSection videos={[{ id: 'v2', title: 'Pizza Secrets' }]} />)
    const article = screen.getByRole('article', { name: 'Play video: Pizza Secrets' })
    fireEvent.click(article)
    expect(screen.getByText('Video unavailable')).toBeDefined()
  })
})

describe('FeaturedVideosSkeleton', () => {
  it('renders skeleton UI', () => {
    const { container } = render(<FeaturedVideosSkeleton />)
    expect(container.firstChild).toBeDefined()
  })
})
