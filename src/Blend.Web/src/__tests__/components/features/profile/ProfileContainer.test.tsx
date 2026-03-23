import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ProfileContainer } from '@/components/features/profile/ProfileContainer'
import type { MyProfile } from '@/types'

vi.mock('next/image', () => ({
  default: ({ alt }: { alt: string }) => <img alt={alt} />,
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

vi.mock('@/hooks/useProfile', () => ({
  useMyProfile: vi.fn(),
  useUpdateProfile: vi.fn(),
  useAvatarUpload: vi.fn(),
  useMyRecipes: vi.fn(),
  useLikedRecipes: vi.fn(),
  useToggleRecipeVisibility: vi.fn(),
  useDeleteRecipe: vi.fn(),
}))

import {
  useMyProfile,
  useUpdateProfile,
  useAvatarUpload,
  useMyRecipes,
  useLikedRecipes,
  useToggleRecipeVisibility,
  useDeleteRecipe,
} from '@/hooks/useProfile'

const mockUseMyProfile = vi.mocked(useMyProfile)
const mockUseUpdateProfile = vi.mocked(useUpdateProfile)
const mockUseAvatarUpload = vi.mocked(useAvatarUpload)
const mockUseMyRecipes = vi.mocked(useMyRecipes)
const mockUseLikedRecipes = vi.mocked(useLikedRecipes)
const mockUseToggleVisibility = vi.mocked(useToggleRecipeVisibility)
const mockUseDeleteRecipe = vi.mocked(useDeleteRecipe)

const mockProfile: MyProfile = {
  id: 'user1',
  displayName: 'Alice Chef',
  email: 'alice@example.com',
  bio: 'Cook',
  joinDate: '2022-01-15T00:00:00Z',
  recipeCount: 10,
  likeCount: 250,
  followerCount: 80,
  followingCount: 45,
}

function setupDefaultMocks() {
  mockUseMyProfile.mockReturnValue({
    data: mockProfile,
    isLoading: false,
    error: null,
  } as unknown as ReturnType<typeof useMyProfile>)
  mockUseUpdateProfile.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
    error: null,
  } as unknown as ReturnType<typeof useUpdateProfile>)
  mockUseAvatarUpload.mockReturnValue({
    mutateAsync: vi.fn(),
    isPending: false,
  } as unknown as ReturnType<typeof useAvatarUpload>)
  mockUseMyRecipes.mockReturnValue({
    data: { pages: [{ recipes: [], hasMore: false }], pageParams: [undefined] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
    isFetchingNextPage: false,
  } as unknown as ReturnType<typeof useMyRecipes>)
  mockUseLikedRecipes.mockReturnValue({
    data: { pages: [{ recipes: [], hasMore: false }], pageParams: [undefined] },
    isLoading: false,
    fetchNextPage: vi.fn(),
    hasNextPage: false,
    isFetchingNextPage: false,
  } as unknown as ReturnType<typeof useLikedRecipes>)
  mockUseToggleVisibility.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useToggleRecipeVisibility>)
  mockUseDeleteRecipe.mockReturnValue({ mutate: vi.fn(), isPending: false } as unknown as ReturnType<typeof useDeleteRecipe>)
}

describe('ProfileContainer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    setupDefaultMocks()
  })

  it('shows loading skeleton while profile is loading', () => {
    mockUseMyProfile.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    } as unknown as ReturnType<typeof useMyProfile>)
    render(<ProfileContainer />)
    expect(screen.getByLabelText('Loading profile')).toBeDefined()
  })

  it('renders profile display name when loaded', () => {
    render(<ProfileContainer />)
    expect(screen.getByText('Alice Chef')).toBeDefined()
  })

  it('renders edit profile button', () => {
    render(<ProfileContainer />)
    expect(screen.getByLabelText('Edit profile')).toBeDefined()
  })

  it('opens edit form when edit button is clicked', () => {
    render(<ProfileContainer />)
    fireEvent.click(screen.getByLabelText('Edit profile'))
    // The dialog should be open
    expect(screen.getByRole('dialog', { name: 'Edit profile' })).toBeDefined()
  })

  it('closes edit form when cancel is clicked', () => {
    render(<ProfileContainer />)
    fireEvent.click(screen.getByLabelText('Edit profile'))
    fireEvent.click(screen.getByLabelText('Cancel editing'))
    expect(screen.queryByRole('dialog', { name: 'Edit profile' })).toBeNull()
  })

  it('shows error state when profile load fails', () => {
    mockUseMyProfile.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 500, message: 'Server error' },
    } as unknown as ReturnType<typeof useMyProfile>)
    render(<ProfileContainer />)
    expect(screen.getByText(/Failed to load profile/)).toBeDefined()
  })

  it('shows sign in message for 401 error', () => {
    mockUseMyProfile.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: { status: 401, message: 'Unauthorized' },
    } as unknown as ReturnType<typeof useMyProfile>)
    render(<ProfileContainer />)
    expect(screen.getByText(/Please sign in/)).toBeDefined()
  })

  it('renders recipe tabs', () => {
    render(<ProfileContainer />)
    expect(screen.getByRole('tab', { name: 'My Recipes' })).toBeDefined()
    expect(screen.getByRole('tab', { name: 'Liked Recipes' })).toBeDefined()
  })
})
