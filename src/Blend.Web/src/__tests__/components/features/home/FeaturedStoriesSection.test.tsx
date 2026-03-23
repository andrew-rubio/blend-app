import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FeaturedStoriesSection, FeaturedStoriesSkeleton } from '@/components/features/home/FeaturedStoriesSection'
import type { HomeFeaturedStory } from '@/types'

const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))
vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockStories: HomeFeaturedStory[] = [
  {
    id: 's1',
    title: 'The Art of Pasta',
    author: 'Jane Doe',
    readingTimeMinutes: 5,
    excerpt: 'Discover the secrets of perfect pasta...',
    coverImageUrl: 'https://example.com/story.jpg',
  },
  {
    id: 's2',
    title: 'Spice Guide',
  },
]

describe('FeaturedStoriesSection', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns null when stories list is empty', () => {
    const { container } = render(<FeaturedStoriesSection stories={[]} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders section heading', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    expect(screen.getByText('Featured Stories')).toBeDefined()
  })

  it('renders story titles', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    expect(screen.getByText('The Art of Pasta')).toBeDefined()
    expect(screen.getByText('Spice Guide')).toBeDefined()
  })

  it('renders author and reading time', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    expect(screen.getByText('Jane Doe')).toBeDefined()
    expect(screen.getByText('5 min read')).toBeDefined()
  })

  it('renders excerpt', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    expect(screen.getByText('Discover the secrets of perfect pasta...')).toBeDefined()
  })

  it('navigates to story detail on click', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    const article = screen.getByRole('article', { name: 'The Art of Pasta' })
    fireEvent.click(article)
    expect(mockPush).toHaveBeenCalledWith('/stories/s1')
  })

  it('navigates via keyboard Enter', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    const article = screen.getByRole('article', { name: 'The Art of Pasta' })
    fireEvent.keyDown(article, { key: 'Enter' })
    expect(mockPush).toHaveBeenCalledWith('/stories/s1')
  })

  it('navigates via keyboard Space', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    const article = screen.getByRole('article', { name: 'The Art of Pasta' })
    fireEvent.keyDown(article, { key: ' ' })
    expect(mockPush).toHaveBeenCalledWith('/stories/s1')
  })

  it('has correct list role', () => {
    render(<FeaturedStoriesSection stories={mockStories} />)
    expect(screen.getByRole('list', { name: 'Featured stories' })).toBeDefined()
  })
})

describe('FeaturedStoriesSkeleton', () => {
  it('renders skeleton UI', () => {
    const { container } = render(<FeaturedStoriesSkeleton />)
    expect(container.firstChild).toBeDefined()
  })
})
