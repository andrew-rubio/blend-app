import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { PublicProfileContainer } from '@/components/features/profile/PublicProfileContainer'
import type { PublicProfile } from '@/types'

vi.mock('next/image', () => ({
  default: ({ alt }: { alt: string }) => <img alt={alt} />,
}))

vi.mock('@/hooks/useProfile', () => ({
  usePublicProfile: vi.fn(),
  usePublicUserRecipes: vi.fn(),
}))

import { usePublicProfile, usePublicUserRecipes } from '@/hooks/useProfile'

const mockUsePublicProfile = vi.mocked(usePublicProfile)
const mockUsePublicUserRecipes = vi.mocked(usePublicUserRecipes)

const mockPublicProfile: PublicProfile = {
  id: 'user2',
  displayName: 'Bob Baker',
  avatarUrl: undefined,
  bio: 'Baker from Paris',
  joinDate: '2020-06-01T00:00:00Z',
  recipeCount: 5,
}

describe('PublicProfileContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUsePublicProfile.mockReturnValue({
      data: mockPublicProfile,
      isLoading: false,
      error: null,
    } as unknown as ReturnType<typeof usePublicProfile>)
    mockUsePublicUserRecipes.mockReturnValue({
      data: { pages: [{ recipes: [], hasMore: false }], pageParams: [undefined] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof usePublicUserRecipes>)
  })

  it('shows loading state', () => {
    mockUsePublicProfile.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as unknown as ReturnType<typeof usePublicProfile>)
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByLabelText('Loading profile')).toBeDefined()
  })

  it('renders display name', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText('Bob Baker')).toBeDefined()
  })

  it('renders bio', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText('Baker from Paris')).toBeDefined()
  })

  it('renders join year', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText(/Member since 2020/)).toBeDefined()
  })

  it('renders recipe count', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText(/5 public recipes/)).toBeDefined()
  })

  it('renders Add friend button', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByLabelText('Add friend')).toBeDefined()
  })

  it('shows empty recipes state', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText(/No public recipes yet/)).toBeDefined()
  })

  it('shows 404 error when user not found', () => {
    mockUsePublicProfile.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 404, message: 'Not found' },
    } as unknown as ReturnType<typeof usePublicProfile>)
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText(/User not found/)).toBeDefined()
  })

  it('shows generic error state', () => {
    mockUsePublicProfile.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 500, message: 'Server error' },
    } as unknown as ReturnType<typeof usePublicProfile>)
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText(/Failed to load profile/)).toBeDefined()
  })

  it('renders recipes when available', () => {
    const recipes = [
      { id: 'r1', title: 'Croissant', imageUrl: undefined, cuisines: ['French'], likeCount: 3, isPublic: true, createdAt: '2024-01-01' },
    ]
    mockUsePublicUserRecipes.mockReturnValue({
      data: { pages: [{ recipes, hasMore: false }], pageParams: [undefined] },
      isLoading: false,
      fetchNextPage: vi.fn(),
      hasNextPage: false,
      isFetchingNextPage: false,
    } as unknown as ReturnType<typeof usePublicUserRecipes>)
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.getByText('Croissant')).toBeDefined()
  })

  it('does not show edit controls', () => {
    render(<PublicProfileContainer userId="user2" />)
    expect(screen.queryByLabelText('Edit profile')).toBeNull()
  })
})
