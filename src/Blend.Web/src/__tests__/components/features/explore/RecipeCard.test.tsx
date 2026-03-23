import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { RecipeCard } from '@/components/features/explore/RecipeCard'
import type { RecipeSearchResult } from '@/types'

// next/image mock
vi.mock('next/image', () => ({
  // eslint-disable-next-line @next/next/no-img-element
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const baseRecipe: RecipeSearchResult = {
  id: '42',
  title: 'Spaghetti Carbonara',
  imageUrl: 'https://example.com/carbonara.jpg',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  readyInMinutes: 30,
  popularity: 200,
  dataSource: 'Spoonacular',
  score: 0.9,
}

describe('RecipeCard', () => {
  it('renders recipe title', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByText('Spaghetti Carbonara')).toBeDefined()
  })

  it('renders the thumbnail image', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByAltText('Spaghetti Carbonara')).toBeDefined()
  })

  it('renders cuisine tags', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByText('Italian')).toBeDefined()
  })

  it('renders ready time', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByLabelText('30 minutes')).toBeDefined()
  })

  it('renders popularity count', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByLabelText('200 likes')).toBeDefined()
  })

  it('renders Spoonacular source badge', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    expect(screen.getByLabelText('Source: Spoonacular')).toBeDefined()
  })

  it('renders Community source badge', () => {
    render(<RecipeCard recipe={{ ...baseRecipe, dataSource: 'Community' }} />)
    expect(screen.getByLabelText('Source: Community')).toBeDefined()
  })

  it('shows placeholder when no imageUrl', () => {
    render(<RecipeCard recipe={{ ...baseRecipe, imageUrl: undefined }} />)
    // No img element with alt text in this case
    expect(screen.queryByAltText('Spaghetti Carbonara')).toBeNull()
  })

  it('calls onClick with recipe id when clicked', () => {
    const handleClick = vi.fn()
    render(<RecipeCard recipe={baseRecipe} onClick={handleClick} />)
    const article = screen.getByRole('article')
    article.click()
    expect(handleClick).toHaveBeenCalledWith('42')
  })

  it('calls onClick when Enter key is pressed', () => {
    const handleClick = vi.fn()
    render(<RecipeCard recipe={baseRecipe} onClick={handleClick} />)
    const article = screen.getByRole('article')
    article.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }))
    expect(handleClick).toHaveBeenCalledWith('42')
  })

  it('is focusable (has tabIndex)', () => {
    render(<RecipeCard recipe={baseRecipe} />)
    const article = screen.getByRole('article')
    expect(article).toHaveAttribute('tabindex', '0')
  })
})
